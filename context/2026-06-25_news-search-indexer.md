# Поиск: воркер индексации + PostgreSQL FTS

**Дата:** 2026-06-25

## Подход
- RSS **не** индексирует и не определяет язык
- Воркер `news-search-indexer` берёт новости с `SearchIndexedAt IS NULL`, **сначала новые**
- Определяет язык (источник → URL-паттерны → кириллица в тексте)
- Пишет в `search_documents`; `tsvector` + GIN через триггер в Postgres
- После обогащения полным текстом `SearchIndexedAt` сбрасывается → переиндексация

## API
`GET /api/search?q=...&limit=20`

## Воркер
`NewsCollector.NewsSearchIndexer` — polling 30s, batch 25.

## Деплой
```bash
docker compose up -d --build api news-search-indexer
```

Миграция: `20260626192112_AddSearchDocuments` (ранее была ручная `20260625140000` без Designer — не применялась).

## UI
Глобальный поиск в шапке (desktop).
