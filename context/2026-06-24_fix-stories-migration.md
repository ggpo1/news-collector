# Fix: миграция Stories / EditorialTags / Embeddings не применялась

## Проблема
API падал: `42703: column n.CategoryUpdatedAt does not exist` при запросе editorial dashboard.

## Причина
Ручные миграции `20260620180000_AddNewsItemEmbeddings.cs` и `20260620190000_AddStoriesAndEditorialTags.cs` были **без** `*.Designer.cs` и без обновления `NewsCollectorDbContextModelSnapshot.cs`.

EF Core не регистрировал их в `dotnet ef migrations list`, `MigrateAsync()` их пропускал.

## Исправление
1. Удалены «осиротевшие» ручные миграции
2. Сгенерирована корректная: `20260624230442_AddEmbeddingsStoriesAndEditorialTags` (+ Designer + snapshot)
3. В Up() добавлены seed editorial_tags, индексы `IX_news_ToneCoefficient` и обновлённый `IX_news_FetchedAt_CategoryPending`
4. Исправлен `NewsItemEmbeddingConfiguration.cs`: `HasConversion` возвращает `void`, value comparer выставляется через отдельную переменную `vectorProperty`

## Статус
- `dotnet build` — OK

## Деплой
```bash
docker compose up -d --build api
```

При `RunMigrations=true` (по умолчанию у api) колонки и таблицы создадутся при старте.

Проверка:
```bash
docker compose logs api | grep -i migr
dotnet ef migrations list  # должна быть 20260624230442_AddEmbeddingsStoriesAndEditorialTags
```
