# Topic linker worker

## Задача
Отдельный воркер `NewsCollector.TopicLinker` раз в минуту ищет связанные новости и пишет их в `news_links`.

## Алгоритм (мульти-сигнальный)
1. Загружает до 300 свежих новостей за последние 168 ч (настраивается).
2. Для каждой пары вычисляет сигналы:
   - **Jaccard** по заголовку + summary (baseline)
   - **Embeddings** — cosine similarity через Ollama `nomic-embed-text`
   - **Сущности** — Jaccard по `NamedEntityId` из `news_entity_mentions`
3. `TopicLinkClassifier` выбирает тип связи:

| Тип | Когда |
|-----|-------|
| `Duplicate` | Jaccard ≥ 0.92 или cosine embedding ≥ 0.93 |
| `SameTopic` | Jaccard ≥ 0.45 (+ min tokens) ИЛИ embedding ≥ 0.78 ИЛИ 2+ сущности + (entity Jaccard ≥ 0.45 или embedding ≥ 0.70) |
| `Related` | embedding ≥ 0.65 ИЛИ 2+ сущности + entity Jaccard ≥ 0.30 — «рядом по теме» |

4. `LinkMethod`: `TitleSimilarity`, `Embedding`, `EntityOverlap`, `Hybrid`
5. Пары канонизируются: `NewsIdLow < NewsIdHigh` (CHECK в БД).

## Проекты
- `NewsCollector.TopicLinker` — hosted service, интервал 60 с
- `TopicLinkingService`, `TopicLinkClassifier`, `TitleSimilarityCalculator`, `EmbeddingSimilarityCalculator`, `EntityOverlapCalculator`
- `OllamaEmbeddingService` — `/api/embed` (fallback на legacy `/api/embeddings`)
- `NewsEmbeddingStore` — кэш в `news_item_embeddings`, batch-запросы к Ollama

## Docker
```bash
docker compose build topic-linker && docker compose up -d topic-linker ollama
```

Сервис `topic-linker` зависит от `ollama` + `ollama-embed-pull` (модель `nomic-embed-text`).

## Конфиг (`TopicLinker` в appsettings)
| Параметр | Default | Описание |
|----------|---------|----------|
| PollingIntervalSeconds | 60 | Интервал цикла |
| LookbackHours | 168 | Окно поиска |
| MaxCandidates | 300 | Лимит новостей за цикл |
| MinSimilarity | 0.45 | Порог Jaccard для SameTopic |
| MinSharedTokens | 2 | Мин. общих слов |
| DuplicateSimilarity | 0.92 | Порог для Duplicate |
| UseEmbeddings | true | Семантика через Ollama |
| UseEntityOverlap | true | Учёт сущностей |
| EmbeddingModel | nomic-embed-text | Модель эмбеддингов |
| MaxEmbeddingsPerCycle | 40 | Новых эмбеддингов за цикл |
| MinEmbeddingSimilarityForRelated | 0.65 | Порог Related по embedding |
| MinSharedEntitiesForRelated | 2 | Мин. общих сущностей для Related |

## API / UI
- `GET /api/news/{id}/related` — все типы связей
- Раздел «Связи» — фильтр по типу, отображение метода
- Карточка новости — блок «Связанные новости»
- Story sync и editorial dashboard используют SameTopic/Duplicate/Related

## Локально
```bash
dotnet run --project src/NewsCollector.TopicLinker
```
