# Ollama host port 11435

## Проблема
`docker compose up` падал: порт `0.0.0.0:11434` уже занят Ollama на хосте.

## Решение
- Внешний проброс контейнера `ollama`: **11435:11434** (дефолт в `docker-compose.yml` и `.env.example`).
- `OLLAMA_BASE_URL=http://ollama:11434` без изменений — API внутри Docker-сети обращается к сервису по внутреннему порту 11434.

## На сервере
Если в `.env` указано `OLLAMA_PORT=11434`, заменить на `11435` или удалить строку (возьмётся дефолт).

```bash
sudo docker compose up -d
```

Для pull модели в контейнер:
```bash
sudo docker compose exec ollama ollama pull deepseek-r1:14b
```
