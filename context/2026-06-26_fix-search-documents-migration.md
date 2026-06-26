# Fix: column s.DefaultLanguage does not exist

## Проблема
API падал при `GET /api/sources`: `42703: column s.DefaultLanguage does not exist`.

## Причина
Ручная миграция `20260625140000_AddSearchDocuments.cs` была **без** `*.Designer.cs` и без обновления `NewsCollectorDbContextModelSnapshot.cs`.

EF Core не видел её в цепочке — `MigrateAsync()` пропускал, колонки в БД не создавались.

## Исправление
1. Удалена осиротевшая `20260625140000_AddSearchDocuments.cs`
2. Сгенерирована корректная: `20260626192112_AddSearchDocuments` (+ Designer + snapshot)
3. `Up()`/`Down()` заменены на идемпотентный SQL (`IF NOT EXISTS`), включая:
   - `sources.DefaultLanguage`
   - `news.Language`, `news.SearchIndexedAt`
   - таблица `search_documents`, GIN-индекс, триггер `tsvector`

## Деплой
```bash
docker compose up -d --build api news-search-indexer
```

При `RunMigrations=true` (по умолчанию у api) миграция применится при старте.

Проверка:
```bash
docker compose logs api | grep -i migr
dotnet ef migrations list  # должна быть 20260626192112_AddSearchDocuments
```

Если старая запись `20260625140000_AddSearchDocuments` уже есть в `__EFMigrationsHistory` (ручной INSERT), удалить её или оставить — новая миграция с другим ID применится отдельно.
