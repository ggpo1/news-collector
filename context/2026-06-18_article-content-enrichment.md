# 2026-06-18 Полный текст статей (content enrichment)

## Проблема

RSS РИА содержит только заголовок и ссылку — без `description` и `content:encoded`. Поле `summary` у всех 96 записей было `NULL`.

## Решение

Двухэтапный сбор:

1. **RSS** — метаданные (title, url, published_at)
2. **Enrichment** — скачивание HTML страницы и извлечение текста по CSS-селектору из `sources.config`

## Схема

- `news.Content` (text) — полный текст статьи
- `news.ContentFetchedAt` — когда пытались загрузить (успех или неудача)

## Конфиг источника (jsonb)

```json
{
  "contentFetchEnabled": true,
  "contentSelector": ".article__text"
}
```

Для РИА селектор `.article__text` — несколько блоков склеиваются через `\n\n`.

## Настройки Worker

- `ContentEnrichmentBatchSize` — статей за цикл (10 prod / 20 dev)
- `ContentEnrichmentDelayMs` — пауза между запросами (500ms prod)

## Проверено

10 из 96 статей обогатились за первый цикл. Остальные подтянутся за ~9 циклов (~9 минут при interval 60s).
