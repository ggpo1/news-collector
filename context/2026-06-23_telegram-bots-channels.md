# 2026-06-23: Telegram-боты, каналы и отправка новостей

## Что сделано

### Backend
- Сущности: `TelegramBot`, `TelegramChannel`, `TelegramChannelCategory`, `TelegramChannelSource`, `TelegramDelivery`
- Миграция `20260623120000_AddTelegram`
- API:
  - `GET/POST/PUT/DELETE /api/telegram/bots` (+ `POST .../restart`)
  - `GET/POST/PUT/DELETE /api/telegram/channels` (фильтр `?sourceId=&categoryId=` для подбора каналов)
  - `POST /api/telegram/news/{id}/send`, `POST /api/telegram/rewrites/{id}/send`
- Очередь `telegram_deliveries` → воркер отправляет через Telegram Bot API (HTML)
- `DockerCliTelegramBotOrchestrator`: при создании бота `docker run` контейнер `nc-telegram-bot-{guid}`
- Проект `NewsCollector.TelegramBot` — воркер с `TelegramBot__BotId`

### Frontend
- Раздел **Telegram** (`/telegram`, chief editor): боты + каналы + маршрутизация по категориям/источникам
- Кнопка **Отправить в Telegram** в карточке новости и переписи (модалка выбора канала)
- В списке новостей ранее добавлен `sourceName`

### Docker
- API: volume `docker.sock`, env `TelegramBotOrchestrator__*`
- Сервис `telegram-bot` (profile `telegram-build`) — сборка образа `news-collector-telegram-bot:latest`

## Деплой

```bash
docker compose --profile telegram-build build telegram-bot
docker compose up -d --build api frontend
```

Проверить `DOCKER_NETWORK` (по умолчанию `news-collector_default` — имя сети compose-проекта).

Бот должен быть админом канала; chat id — `-100...` или `@channel`.

## Ограничения
- Оркестратор требует Docker socket на хосте API
- Без Docker контейнер не стартует (статус Error), очередь копится
- Токены в БД в открытом виде (маскируются только в API-ответах)
