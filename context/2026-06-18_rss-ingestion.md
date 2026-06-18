# 2026-06-18 RSS-парсер и фоновый сбор новостей

## Что сделано

- `NewsCollector.Application` — интерфейсы `IRssFeedReader`, `INewsIngestionService`, модель `ParsedFeedItem`
- `RssFeedReader` — HTTP + `System.ServiceModel.Syndication`, сохранение raw JSON
- `NewsIngestionService` — опрос источников по `FetchIntervalMinutes`, дедуп по `external_id`
- `CollectorWorker` — цикл опроса каждые `Collector:PollingIntervalSeconds` (60s prod / 30s dev)

## Особенности

- РИА блокирует запросы без browser User-Agent (QRATOR WAF) — в HttpClient задан Chrome UA
- При ошибке одного источника остальные продолжают обрабатываться
- `LastFetchedAt` обновляется даже при пустом фиде

## Проверено

- Docker: 96 новостей из `https://ria.ru/export/rss2/archive/index.xml` в таблице `news`

## Следующий шаг

- Добавить источники (Lenta, Interfax и т.д.)
- Алгоритм связывания статей (`news_links`)
- HTTP API для просмотра новостей
