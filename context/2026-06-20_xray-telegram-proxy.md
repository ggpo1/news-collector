# 2026-06-20: Xray proxy для Telegram из Docker

## Проблема
Контейнер `nc-telegram-bot-*` не достаёт до `api.telegram.org`: Xray на хосте слушает `127.0.0.1:10808`, loopback недоступен из Docker.

## Решение (Вариант А)
1. Xray inbounds: `listen: 0.0.0.0` (скрипт `scripts/setup-xray-docker-proxy.sh`)
2. В `.env`: HTTP proxy на **10809** (не SOCKS 10808 — .NET HttpClient не поддерживает SOCKS5)
3. Оркестратор передаёт прокси в `docker run` + `--add-host=host.docker.internal:host-gateway`
4. `TelegramHttpClientExtensions` — явный `WebProxy` для HttpClient "Telegram"

## Переменные
```
TELEGRAM_WORKER_HTTP_PROXY=http://host.docker.internal:10809
TELEGRAM_WORKER_HTTPS_PROXY=http://host.docker.internal:10809
```

## Деплой
```bash
sudo ./scripts/setup-xray-docker-proxy.sh
# добавить переменные в .env
docker compose up -d --build api telegram-bot
# Telegram UI → Restart у ботов
```
