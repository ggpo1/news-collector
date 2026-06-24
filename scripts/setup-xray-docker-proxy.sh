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
        if (.protocol == "socks" or .protocol == "http") and (.listen == "127.0.0.1" or .listen == "localhost")
        then .listen = "0.0.0.0"
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
echo "==> Проверка с хоста"
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

echo ""
echo "==> Проверка из Docker-сети (как контейнер)"
docker run --rm --add-host=host.docker.internal:host-gateway curlimages/curl:8.5.0 \
  curl -fsS --max-time 15 -x "http://host.docker.internal:10809" "https://api.telegram.org" \
  && echo "  Контейнер → host.docker.internal:10809 → Telegram: OK" \
  || echo "  Контейнер → proxy: FAIL (firewall? ufw allow from 172.17.0.0/16?)"

echo ""
echo "================================================================"
echo "Переменные для .env (HTTP proxy — для .NET HttpClient):"
echo ""
echo "  TELEGRAM_WORKER_HTTP_PROXY=http://host.docker.internal:10809"
echo "  TELEGRAM_WORKER_HTTPS_PROXY=http://host.docker.internal:10809"
echo ""
echo "Альтернатива через IP bridge:"
echo "  TELEGRAM_WORKER_HTTP_PROXY=http://${DEFAULT_BRIDGE_IP}:10809"
echo "  TELEGRAM_WORKER_HTTPS_PROXY=http://${DEFAULT_BRIDGE_IP}:10809"
echo ""
echo "После обновления .env:"
echo "  docker compose up -d --build api telegram-bot"
echo "  # В UI Telegram → Restart у каждого бота (пересоздаст контейнеры воркеров)"
echo "================================================================"
