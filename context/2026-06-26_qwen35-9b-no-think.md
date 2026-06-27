# Переход на qwen3.5:9b + think: false везде

**Дата:** 2026-06-26

## Изменения
- Дефолт `OLLAMA_MODEL=qwen3.5:9b` (был 27b)
- Все `/api/chat` через `OllamaChatHelper` с **`"think": false`** (жёстко, без env)
- Модули: rewrite, brief, second-day angles, categorizer, tone, NER
- Удалён `OllamaOptions.Think` и `OLLAMA_THINK`

## Деплой
```bash
# .env
OLLAMA_MODEL=qwen3.5:9b

docker compose up -d --build api editorial-brief ollama ollama-chat-pull
docker compose exec ollama ollama pull qwen3.5:9b
```

## Память
qwen3.5:9b Q4 — ~6–8 GB VRAM (vs ~16+ GB у 27b).
