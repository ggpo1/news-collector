# API visit statistics (api_visits)

## Задача
Таблица в PostgreSQL для статистики обращений к API: какой ресурс, когда, от какого «посетителя».

## Таблица `api_visits`
| Поле | Тип | Описание |
|------|-----|----------|
| Id | uuid | PK |
| RequestedAt | timestamptz | время запроса |
| HttpMethod | varchar(16) | GET, POST, … |
| Path | varchar(2048) | путь, напр. `/api/news` |
| QueryString | varchar(2048)? | query-параметры |
| StatusCode | int | HTTP-ответ |
| DurationMs | int | длительность обработки |
| VisitorFingerprint | varchar(64) | SHA-256 отпечаток посетителя |
| UserAgent | varchar(512)? | UA (обрезанный) |
| UserId | uuid? | FK → `users`, пользователь из JWT (NULL для анонимных запросов) |

Индексы: `RequestedAt`, `(VisitorFingerprint, RequestedAt)`, `(Path, RequestedAt)`, `(UserId, RequestedAt)`.

## Отпечаток посетителя
1. **С фронта** — заголовок `X-Visitor-Id` (UUID в `localStorage`), хеш `sha256("cid:{uuid}")`. Стабилен для браузера, даже за NAT.
2. **Без заголовка** (curl, боты) — `sha256("ip:{ip}|ua:{userAgent}|lang:{acceptLanguage}")`, IP из `X-Forwarded-For` / `X-Real-IP`.

Сырой IP в БД не хранится — только в составе хеша fallback-отпечатка.

## Реализация
- `ApiVisitTrackingMiddleware` — логирует только пути `/api/*`
- `IApiVisitWriter` / `ApiVisitWriter` — запись в БД
- Фронт: `visitor-id.ts` + заголовок в `client.ts`
- nginx проксирует `X-Visitor-Id`

Миграция: `AddApiVisits`.

## Деплой
Worker применяет миграции (`RunMigrations=true`). Пересборка: `docker compose build api frontend worker && docker compose up -d`.

## Пример SQL
```sql
SELECT path, COUNT(*) AS hits, COUNT(DISTINCT "VisitorFingerprint") AS visitors
FROM api_visits
WHERE "RequestedAt" >= NOW() - INTERVAL '7 days'
GROUP BY path
ORDER BY hits DESC;
```
