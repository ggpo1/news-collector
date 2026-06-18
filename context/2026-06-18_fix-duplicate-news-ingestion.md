# Fix: duplicate key on news ingestion (IX_news_SourceId_ExternalId)

## Проблема
Worker падал с `DbUpdateException` / Postgres `23505` при массовой вставке новостей из RSS (РИА Новости). В одном батче ~200 INSERT, затем ошибка unique constraint `IX_news_SourceId_ExternalId`.

## Причина
`NewsIngestionService` проверял только уже существующие `ExternalId` в БД, но не:
- дубликаты **внутри одного RSS-фида** (один и тот же `ExternalId` или URL встречался дважды);
- коллизии по `(SourceId, Url)` при разных `ExternalId`.

## Решение
1. **`FeedItemDeduplicator`** — дедупликация элементов фида по `ExternalId` и `Url` (оставляем более свежий по `PublishedAt`).
2. **`NewsIngestionService`** — перед вставкой фильтруем и по существующим `ExternalId`, и по `Url` в БД.
3. **Fallback при конфликте** — если bulk `SaveChanges` ловит `23505`, вставка по одной записи с пропуском дубликатов; `LastFetchedAt` источника обновляется в любом случае.
4. **`RssFeedReader`** — trim для `ExternalId` и URL.

## Файлы
- `src/NewsCollector.Infrastructure/Feeds/FeedItemDeduplicator.cs` (новый)
- `src/NewsCollector.Infrastructure/Services/NewsIngestionService.cs`
- `src/NewsCollector.Infrastructure/Feeds/RssFeedReader.cs`

## Деплой
Пересобрать worker: `docker compose build worker && docker compose up -d worker`
