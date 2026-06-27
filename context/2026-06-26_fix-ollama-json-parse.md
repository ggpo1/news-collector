# Fix: parse Ollama rewrite JSON (depth not zero)

## Симптом
Перепись завершалась за ~664 с, но падала на парсинге:
`Expected depth to be zero... BytePositionInLine: 809` при ResponseLength=459.

## Причина
`ExtractJson` брал подстроку от первого `{` до **последнего** `}`.
Если qwen3.5 после валидного JSON добавлял хвост (второй объект, thinking, мусор) — парсер получал битый JSON.

## Исправление
- `OllamaJsonExtractor.ExtractFirstJsonObject` — балансировка `{`/`}` с учётом строк и escape
- Все Ollama JSON-парсеры переведены на общий extractor
- Rewrite: fallback на `message.thinking`, если `content` пуст

## Деплой
```bash
docker compose up -d --build api
```
