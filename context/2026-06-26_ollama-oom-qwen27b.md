# Ollama OOM: signal killed на qwen3.5:27b

**Дата:** 2026-06-26

## Симптом
```
Ollama returned 500: {"error":"llama-server process has terminated: signal: killed"}
```
Через ~90 с на `/api/chat` (бриф, перепись).

## Причина
Linux OOM-killer убивает `llama-server` — не хватает RAM/VRAM для 27B + контекста.
На том же инстансе `ollama` ещё держится `nomic-embed-text` (topic-linker).

## Изменения в коде
- `OllamaOptions`: `NumCtx` (8192), `KeepAlive` (5m), `Think` (false для qwen3)
- `OllamaChatHelper` — единый запрос + понятное сообщение при OOM
- `docker-compose`: `OLLAMA_MAX_LOADED_MODELS=1`, `OLLAMA_NUM_PARALLEL=1`

## Что сделать на сервере

### 1. Проверить память
```bash
free -h
nvidia-smi   # если GPU
docker compose logs ollama | tail -50
dmesg | grep -i oom | tail -5
```

### 2. Обновить `.env` (если <16 GB VRAM)
```env
OLLAMA_MODEL=qwen2.5:14b
# или
OLLAMA_MODEL=llama3.1:8b

OLLAMA_NUM_CTX=8192
OLLAMA_MAX_LOADED_MODELS=1
OLLAMA_KEEP_ALIVE=5m
```

### 3. Пересобрать
```bash
docker compose pull ollama
docker compose up -d --build api editorial-brief ollama ollama-chat-pull
```

### 4. Не гонять бриф и перепись одновременно
Один тяжёлый запрос на инстанс `ollama` за раз.

## Требования qwen3.5:27b
~16+ GB VRAM (Q4). На CPU-only 27B часто падает по OOM.
