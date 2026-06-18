# 2026-06-18 Docker Compose: полная развёртка

## Что сделано

- `Dockerfile` — multi-stage сборка Worker (.NET 8 runtime)
- `docker-compose.yml` — postgres + worker с healthcheck и depends_on
- `RunMigrations=true` в контейнере — EF миграции при старте worker
- `.dockerignore` — исключены bin/obj, context, .env

## Запуск

```bash
cp .env.example .env   # опционально, есть дефолты
docker compose up -d --build
docker compose logs -f worker
```

Остановка: `docker compose down` (данные в volume `postgres_data` сохраняются).

## Переменные

| Переменная | По умолчанию | Назначение |
|------------|--------------|------------|
| POSTGRES_PORT | 5433 | Порт Postgres на хосте |
| POSTGRES_DB/USER/PASSWORD | news_collector/news/news | Учётные данные БД |
| RUN_MIGRATIONS | true | Применять миграции при старте worker |
| ASPNETCORE_ENVIRONMENT | Production | Окружение .NET |
