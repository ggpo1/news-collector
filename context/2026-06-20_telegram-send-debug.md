# 2026-06-20: Диагностика отправки Telegram-сообщений

## Симптом
Воркер `NewsCollector.TelegramBot` находит `Pending` записи в `telegram_deliveries`, вызывает `sendMessage`, но статус меняется на `Failed` с `ErrorMessage`.

## Вывод
Инфраструктура работает: контейнер бота запущен, БД доступна, очередь обрабатывается. Проблема на стороне Telegram API (неверный chat_id, бот не админ канала, ошибка HTML-разметки и т.д.).

## Что сделано в коде

1. **TelegramApiClient** — читает полный ответ API, логирует `description`, при ошибке HTML (`can't parse entities`) повторяет отправку plain text; `chat_id` для числовых ID уходит как number в JSON.
2. **TelegramDeliveryService** — `LogWarning` с `DeliveryId`, `ChatId` и текстом ошибки.
3. **TelegramChannelService** — при создании/редактировании канала вызывает `getChat` (бот должен видеть канал).
4. **API** — `GET /api/telegram/deliveries/{id}` для статуса доставки.
5. **Frontend** — модалка отправки опрашивает статус и показывает реальную ошибку Telegram.

## Как проверить на сервере

```sql
SELECT "Status", "ErrorMessage", "CreatedAt", "SentAt"
FROM telegram_deliveries
ORDER BY "CreatedAt" DESC
LIMIT 10;
```

```bash
docker logs nc-telegram-bot-<BOT_GUID> 2>&1 | grep -i "Telegram delivery\|sendMessage failed"
```

## Типичные ошибки Telegram

| Ошибка | Решение |
|--------|---------|
| `chat not found` | Неверный chat id; для каналов нужен `-100...` или `@username` |
| `bot is not a member` / `need administrator` | Добавить бота админом канала |
| `can't parse entities` | Исправлено fallback на plain text (после деплоя) |
| `Unauthorized` | Неверный токен бота |

## Деплой

```bash
docker compose up -d --build api frontend telegram-bot
# перезапустить бота в UI (Telegram → Restart) чтобы поднялся новый образ воркера
```
