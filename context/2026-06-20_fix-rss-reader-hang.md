# Fix: worker зависает на LA Times RSS

## Симптом
После `Collecting news from latimes` и HTTP 200 — тишина, следующий цикл через 60 с не начинается.

## Причина
1. **`SyndicationFeed.Load` + `XmlReader.Async = true` на сетевом потоке** — синхронный парсер может зависнуть навсегда (deadlock), HttpClient timeout при этом не срабатывает.
2. **LA Times RSS** — тело отдаётся очень медленно (throttling), в item'ах большой `content:encoded`.

## Решение
- `RssFeedReader`: сначала буферизовать ответ в `MemoryStream` (лимит 10 MB), парсить с `Async = false`.
- Обрезка summary/rawPayload.
- HttpClient timeout RSS: 120 с.
- Лог `Parsed N items from {url} ({bytes} bytes)`.
- `CollectorWorker`: лог `Cycle completed` каждый цикл.

## Деплой
```bash
docker compose up -d --build worker
```
