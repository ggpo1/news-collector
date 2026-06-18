# 2026-06-18 News Collector: схема БД и каркас проекта

## Что сделано

Создано ASP.NET Core 8 решение с тремя проектами:

- `NewsCollector.Domain` — сущности и enum'ы
- `NewsCollector.Infrastructure` — EF Core DbContext, Fluent API, миграции
- `NewsCollector.Worker` — хост, DI, авто-миграция в Development

## Таблицы

1. **sources** — источники (RSS/Html/Api), seed: РИА Новости
2. **news** — сырые новости с дедупликацией по `(source_id, external_id)` и `(source_id, url)`
3. **news_links** — связи между новостями с каноническим порядком `NewsIdLow < NewsIdHigh`

## Инфраструктура

- `docker-compose.yml` — PostgreSQL 16 на порту **5433** (5432 был занят)
- `.env.example` — переменные окружения
- Миграция `InitialCreate` применена

## Запуск

```bash
docker compose up -d
dotnet ef database update --project src/NewsCollector.Infrastructure --startup-project src/NewsCollector.Worker
dotnet run --project src/NewsCollector.Worker
```

## Следующий шаг

RSS-парсер и BackgroundService для опроса активных источников.
