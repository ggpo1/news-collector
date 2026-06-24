# 2026-06-25: Outbound proxy для worker (RSS)

## Изменения
- `OutboundProxyOptions` + `HttpClientProxyExtensions` — общий HTTP-прокси для исходящих запросов
- Worker: `RssFeedReader` и `ArticleFetcher` через Xray (порт 10809)
- `docker-compose.yml`: `OUTBOUND_PROXY`, `extra_hosts` для worker
- Telegram использует тот же механизм (`TELEGRAM_PROXY` / fallback на `OUTBOUND_PROXY`)

## .env
```
OUTBOUND_PROXY=http://host.docker.internal:10809
TELEGRAM_PROXY=http://host.docker.internal:10809
```

## Деплой
```bash
docker compose up -d --build worker
docker compose logs worker 2>&1 | grep -i proxy
# Outbound HTTP proxy configured: http://host.docker.internal:10809
```
