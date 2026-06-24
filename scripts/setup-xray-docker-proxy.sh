#!/usr/bin/env bash
# Настройка Xray на хосте для доступа Docker-контейнеров к Telegram API.
#
# Вариант А: Xray слушает 0.0.0.0 (не только 127.0.0.1), контейнеры ходят на IP хоста.
#
# Важно для .NET (NewsCollector.TelegramBot):
#   HttpClient поддерживает HTTP-прокси, но НЕ SOCKS5 из коробки.
#   Используйте HTTP inbound Xray (порт 10809), не SOCKS (10808).
#
# Usage:
#   sudo ./scripts/setup-xray-docker-proxy.sh
#   sudo ./scripts/setup-xray-docker-proxy.sh --dry-run

set -euo pipefail

XRAY_CONFIG="${XRAY_CONFIG:-/usr/local/etc/xray/config.json}"
DRY_RUN=false

for arg in "$@"; do
  case "$arg" in
    --dry-run) DRY_RUN=true ;;
    -h|--help)
      sed -n '2,12p' "$0"
      exit 0
      ;;
  esac
done

if [[ "${EUID:-$(id -u)}" -ne 0 ]]; then
  echo "Запустите с sudo: sudo $0"
  exit 1
fi

if [[ ! -f "$XRAY_CONFIG" ]]; then
  echo "Конфиг Xray не найден: $XRAY_CONFIG"
  exit 1
fi

BACKUP="${XRAY_CONFIG}.bak.$(date +%Y%m%d_%H%M%S)"
echo "==> Бэкап: $BACKUP"
if [[ "$DRY_RUN" == false ]]; then
  cp "$XRAY_CONFIG" "$BACKUP"
fi

patch_listen() {
  local file="$1"
  if command -v jq >/dev/null 2>&1; then
    jq '
      .inbounds |= map(
        if (.port == 10808 or .port == 10809)
           or (.protocol == "socks" or .protocol == "http" or .protocol == "mixed")
        then . + {"listen": "0.0.0.0"}
        else .
        end
      )
    ' "$file"
  else
    echo "jq не установлен — правим sed (установите jq для надёжности: apt install jq)" >&2
    sed -E 's/"listen"[[:space:]]*:[[:space:]]*"127\.0\.0\.1"/"listen": "0.0.0.0"/g' "$file"
  fi
}

echo "==> Меняем listen 127.0.0.1 → 0.0.0.0 для inbounds (socks/http)"
NEW_CONFIG="$(patch_listen "$XRAY_CONFIG")"

if [[ "$DRY_RUN" == true ]]; then
  echo "--- Новый конфиг (preview) ---"
  echo "$NEW_CONFIG" | head -80
  echo "..."
  exit 0
fi

echo "$NEW_CONFIG" > "$XRAY_CONFIG"

echo "==> Перезапуск Xray"
systemctl restart xray
systemctl --no-pager --full status xray | head -15

echo ""
echo "==> IP хоста для Docker"

DOCKER0_IP=""
if ip -4 addr show docker0 >/dev/null 2>&1; then
  DOCKER0_IP="$(ip -4 addr show docker0 | awk '/inet / {print $2}' | cut -d/ -f1 | head -1)"
fi

DEFAULT_BRIDGE_IP="${DOCKER0_IP:-172.17.0.1}"

echo "  docker0 (bridge):     ${DEFAULT_BRIDGE_IP}"
echo "  host.docker.internal: используйте в docker run / compose (рекомендуется)"

echo ""
echo "==> Проверка: Xray слушает на всех интерфейсах"
if command -v ss >/dev/null 2>&1; then
  ss -tlnp | grep -E ':1080[89]' || echo "  Порты 10808/10809 не найдены в ss -tlnp"
elif command -v netstat >/dev/null 2>&1; then
  netstat -tlnp | grep -E ':1080[89]' || true
fi
echo "  Ожидается: 0.0.0.0:10809 (не 127.0.0.1:10809)"
if curl -fsS --max-time 10 -x "http://127.0.0.1:10809" "https://api.telegram.org" >/dev/null 2>&1; then
  echo "  HTTP proxy 127.0.0.1:10809 → api.telegram.org: OK"
else
  echo "  HTTP proxy 127.0.0.1:10809: FAIL (проверьте inbound http на 10809)"
fi

if curl -fsS --max-time 10 --socks5-hostname 127.0.0.1:10808 "https://api.telegram.org" >/dev/null 2>&1; then
  echo "  SOCKS5 127.0.0.1:10808 → api.telegram.org: OK"
else
  echo "  SOCKS5 127.0.0.1:10808: FAIL"
fi

echo "==> Firewall (ufw): разрешить Docker → Xray"
if command -v ufw >/dev/null 2>&1 && ufw status 2>/dev/null | grep -q "Status: active"; then
  ufw allow from 172.16.0.0/12 to any port 10809 comment 'Docker to Xray HTTP' || true
  ufw allow from 172.16.0.0/12 to any port 10808 comment 'Docker to Xray SOCKS' || true
  ufw reload || true
  echo "  ufw: правила для 172.16.0.0/12 → 10808/10809 добавлены"
else
  echo "  ufw не активен — если Docker всё равно не достучится, проверьте iptables INPUT"
fi

echo ""
echo "==> Проверка из Docker-сети (как контейнер)"
COMPOSE_NETWORK="${DOCKER_NETWORK:-news-collector_default}"
NETWORK_ARG=""
if docker network inspect "$COMPOSE_NETWORK" >/dev/null 2>&1; then
  NETWORK_ARG="--network ${COMPOSE_NETWORK}"
  echo "  Используем сеть: ${COMPOSE_NETWORK}"
else
  echo "  Сеть ${COMPOSE_NETWORK} не найдена — default bridge"
fi

test_from_docker() {
  local label="$1"
  local proxy_url="$2"
  if docker run --rm ${NETWORK_ARG} --add-host=host.docker.internal:host-gateway curlimages/curl:8.5.0 \
    curl -fsS --max-time 15 -x "${proxy_url}" "https://api.telegram.org" >/dev/null 2>&1; then
    echo "  ${label}: OK"
    return 0
  else
    echo "  ${label}: FAIL"
    return 1
  fi
}

DOCKER_OK=false
test_from_docker "host.docker.internal:10809" "http://host.docker.internal:10809" && DOCKER_OK=true
test_from_docker "172.17.0.1:10809 (docker0)" "http://${DEFAULT_BRIDGE_IP}:10809" && DOCKER_OK=true

if [[ "$DOCKER_OK" == false ]]; then
  echo ""
  echo "  TCP probe (busybox nc):"
  docker run --rm ${NETWORK_ARG} --add-host=host.docker.internal:host-gateway busybox:1.36 \
    sh -c "nc -zv -w 5 host.docker.internal 10809 && nc -zv -w 5 ${DEFAULT_BRIDGE_IP} 10809" \
    || echo "  TCP to proxy port failed — Xray still on 127.0.0.1 or firewall DROP"
fi

echo ""
echo "================================================================"
echo "Переменные для .env (HTTP proxy — для .NET HttpClient):"
echo ""
echo "  TELEGRAM_PROXY=http://host.docker.internal:10809"
echo ""
echo "Альтернатива через IP bridge:"
echo "  TELEGRAM_PROXY=http://${DEFAULT_BRIDGE_IP}:10809"
echo ""
echo "После обновления .env:"
echo "  docker compose up -d --build api telegram-bot"
echo "  # В UI Telegram → Restart у каждого бота (пересоздаст контейнеры воркеров)"
echo "================================================================"
