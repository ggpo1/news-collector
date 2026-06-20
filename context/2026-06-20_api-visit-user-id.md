# api_visits: UserId

## Задача
Добавить в `api_visits` идентификатор пользователя, который запрашивал ресурс.

## Изменения
- `ApiVisit.UserId` (uuid?, FK → `users`, ON DELETE SET NULL)
- `ApiVisitEntry.UserId` — передаётся из middleware
- `ApiVisitTrackingMiddleware` — читает `ClaimTypes.NameIdentifier` из JWT после обработки запроса
- Индекс `(UserId, RequestedAt)` для выборок по пользователю
- Миграция: `AddApiVisitUserId`

## Поведение
- Авторизованный запрос → `UserId` заполнен
- Анонимные эндпоинты (`/api/auth/login`, `register`, `validate-invitation`) → `UserId` = NULL

## Пример SQL
```sql
SELECT u."Login", v."Path", COUNT(*) AS hits
FROM api_visits v
JOIN users u ON u."Id" = v."UserId"
WHERE v."RequestedAt" >= NOW() - INTERVAL '7 days'
GROUP BY u."Login", v."Path"
ORDER BY hits DESC;
```

## Деплой
```bash
docker compose up -d --build
```
