# Переход на qwen3.5:27b (rewrite + бриф)

**Дата:** 2026-06-25

## Изменение
Заменили `deepseek-r1:14b` на `qwen3.5:27b` для:
- AI-переписывания новостей (`OllamaAiNewsRewriteService`, API)
- редакционного брифа (`EditorialBriefGenerator`, воркер `editorial-brief`)

Оба используют один `OLLAMA_MODEL` на инстансе `ollama`.

## Файлы
- `.env.example` — `OLLAMA_MODEL=qwen3.5:27b`
- `docker-compose.yml` — дефолты api/editorial-brief + сервис `ollama-chat-pull`
- `OllamaOptions.cs`, `appsettings.json` (Api, EditorialBrief)

## Деплой
```bash
# обновить .env на сервере
OLLAMA_MODEL=qwen3.5:27b

docker compose up -d ollama ollama-chat-pull api editorial-brief
```

Или вручную:
```bash
docker compose exec ollama ollama pull qwen3.5:27b
docker compose up -d api editorial-brief
```

## Требования
- **VRAM:** ~16+ GB для Q4-квантизации 27B
- Таймаут оставлен 1800 с

## Примечание
Second-day angles в API тоже используют `OLLAMA_MODEL` — автоматически перейдут на qwen3.5:27b.
