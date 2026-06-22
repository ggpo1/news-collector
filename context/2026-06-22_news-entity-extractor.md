# News entity extractor — семантическая карта

## Задача
Извлекать из новостей именованные сущности (персоны, компании, страны, организации, локации, события) для построения графа совместных упоминаний.

## Схема БД

| Таблица | Назначение |
|---------|------------|
| `named_entities` | Канонические сущности с `NormalizedKey` для дедупликации |
| `news_entity_mentions` | Связь новость ↔ сущность (уникальность по паре) |
| `news.EntitiesExtractedAt` | Маркер обработки (как `ToneAnalyzedAt`) |

Типы: `person`, `company`, `country`, `organization`, `location`, `event`, `other`.

## Воркер `news-entity-extractor`
- Ollama JSON: `{"entities":[{"name":"...","type":"person"}]}`
- Batch 5 (меньше categorizer/tone — тяжелее промпт)
- Пропускает новости без `Content` и `Summary`
- Пустой список сущностей тоже помечает `EntitiesExtractedAt`

## API (для визуализации)
- `GET /api/entities/graph?from=&to=&type=&minWeight=2&maxNodes=150` — узлы + рёбра (co-mention weight)
- `GET /api/entities?q=&type=&limit=50` — поиск сущностей
- `GET /api/entities/{id}?from=&to=` — соседи по совместным упоминаниям

## Деплой
```bash
docker compose up -d --build api news-entity-extractor ollama-ner ollama-ner-pull
```

NER использует **отдельный** Ollama (`ollama-ner`) с лёгкой моделью `llama3.2:3b`, не конкурирует с `deepseek-r1` на основном `ollama`.

Переменные: `OLLAMA_NER_BASE_URL`, `OLLAMA_NER_MODEL`, `OLLAMA_NER_TIMEOUT_SECONDS` (дефолт 600 с).
Порт на хосте: `OLLAMA_NER_PORT=11436`.

## Проверка
```sql
SELECT COUNT(*) FROM named_entities;
SELECT COUNT(*) FROM news_entity_mentions;
SELECT COUNT(*) FROM news WHERE "EntitiesExtractedAt" IS NOT NULL;
```

## Следующий шаг (frontend)
Страница **Карта** (`/map`) — force-directed граф на canvas:
- фильтры: период (1/7/30 дней), тип сущности, мин. вес связи
- zoom/pan, клик по узлу → панель соседей
- API: `GET /api/entities/graph`
