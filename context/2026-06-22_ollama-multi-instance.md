# Ollama: отдельные инстансы по задачам

## Топология

| Сервис Docker | Воркер / клиент | Порт хоста | Модель по умолчанию |
|---------------|-----------------|------------|---------------------|
| `ollama` | API (rewrite, second-day), editorial-brief | 11435 | `qwen3.5:27b` |
| `ollama-ner` | `news-entity-extractor` | 11436 | `llama3.2:3b` |
| `ollama-categorizer` | `news-categorizer` | 11437 | `llama3.2:3b` |
| `ollama-tone` | `news-tone-analyzer` | 11438 | `llama3.2:3b` |

У каждого инстанса свой volume (`ollama_*_data`). Pull-модели: `ollama-*-pull` (одноразовые контейнеры при `docker compose up`).

## Деплой воркеров с их Ollama

```bash
docker compose up -d \
  ollama-categorizer ollama-categorizer-pull news-categorizer \
  ollama-tone ollama-tone-pull news-tone-analyzer \
  ollama-ner ollama-ner-pull news-entity-extractor
```

## Переменные `.env`

- `OLLAMA_*` — только API rewrite
- `OLLAMA_CATEGORIZER_*`, `OLLAMA_TONE_*`, `OLLAMA_NER_*` — соответствующие воркеры

Таймаут лёгких моделей: **600 с** (вместо 1800 у тяжёлой).
