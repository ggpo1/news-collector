# Docker Compose: profile ollama для UI-разработки

**Дата:** 2026-06-25

## Изменение
Ollama-инстансы, pull-контейнеры и AI-воркеры вынесены в `profiles: [ollama]`.

По умолчанию `docker compose up -d` поднимает: postgres, api, frontend, worker, telegram-bot (build).

## Команды
```bash
# Фронт / дизайн
docker compose up -d postgres api frontend

# + RSS-данные
docker compose up -d postgres api frontend worker

# Полный стек
docker compose --profile ollama up -d
```

AI-кнопки (переписать, бриф, углы) без profile не работают — API стартует без Ollama.
