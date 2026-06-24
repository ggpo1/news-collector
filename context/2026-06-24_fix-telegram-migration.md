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

Ручная проверка:
```bash
docker compose exec api dotnet ef database update  # если нужно
# или SQL: SELECT * FROM "__EFMigrationsHistory" WHERE "MigrationId" LIKE '%AddTelegram%';
```
