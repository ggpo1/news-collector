# 2026-06-24: Исправление миграции Telegram-таблиц

## Проблема
API падал с `42P01: relation "telegram_bots" / "telegram_channels" does not exist`.

## Причина
Ручная миграция `20260623120000_AddTelegram.cs` была без:
- `[Migration(...)]` / `[DbContext(...)]`
- файла `*.Designer.cs`
- обновления `NewsCollectorDbContextModelSnapshot.cs`

EF Core не видел миграцию в списке (`dotnet ef migrations list`), `MigrateAsync()` её не применял.

## Исправление
1. Удалена сломанная миграция `20260623120000_AddTelegram.cs`
2. Сгенерирована корректная: `20260624193345_AddTelegram` (+ Designer + snapshot)

## Деплой на сервер
```bash
docker compose up -d --build api
# или полный стек:
docker compose --profile telegram-build build telegram-bot
docker compose up -d --build api frontend
```

При `RunMigrations=true` (по умолчанию в compose) таблицы создадутся при старте API/worker.

## Исправление (2026-06-24, гонка миграций)

Ошибка `23505 pg_type_typname_nsp_index` при `CREATE TABLE telegram_bots`:
- несколько контейнеров одновременно вызывали `MigrateAsync` (api + worker + categorizer + …)
- после сбоя оставались «осиротевшие» типы в `pg_type` без таблиц

**Фикс:**
1. Миграция `AddTelegram` переведена на `CREATE TABLE IF NOT EXISTS` + очистка orphan types
2. `DatabaseMigrationExtensions.MigrateWithAdvisoryLockAsync()` — advisory lock на всех сервисах
3. В `docker-compose.yml` миграции по умолчанию только у **api** (`RunMigrations=true`), у воркеров — `false`

Деплой:
```bash
docker compose up -d --build api
```

Если миграция всё ещё падает вручную в psql:
```sql
DROP TYPE IF EXISTS telegram_bots CASCADE;
DROP TYPE IF EXISTS telegram_channels CASCADE;
-- затем перезапуск api
```
