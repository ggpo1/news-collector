# Фильтр тональности + улучшенный topic linking

**Дата:** 2026-06-20

## 1. Фильтр тональности в списке новостей

### Backend
- `NewsToneFilter` enum: `Positive`, `Negative`, `Neutral`, `Strong`, `Unanalyzed`
- `GET /api/news?toneFilter=positive|negative|neutral|strong|unanalyzed`
- Пороги совпадают с редакционным дашбордом: ±0.30 / |тон| ≥ 0.55
- Индекс `IX_news_ToneCoefficient`

### Frontend
- Компонент `ToneSelect` рядом с `CategorySelect` на странице `/news`
- Hook `useNews` передаёт `toneFilter` в API

## 2. Улучшенный topic linking

### Новые сигналы
1. **Jaccard по заголовку** — baseline (как раньше)
2. **Embeddings** — Ollama `/api/embeddings`, модель `nomic-embed-text`, хранение в `news_item_embeddings`
3. **Сущности** — Jaccard по `NamedEntityId` из `news_entity_mentions`

### Типы связей
| Тип | Когда |
|-----|-------|
| `Duplicate` | Jaccard ≥ 0.92 или cosine embedding ≥ 0.93 |
| `SameTopic` | Jaccard ≥ 0.45 (+ min tokens) ИЛИ embedding ≥ 0.78 ИЛИ 2+ общих сущности + embedding ≥ 0.70 |
| `Related` | embedding ≥ 0.65 ИЛИ 2+ сущности + entity Jaccard ≥ 0.30 — «рядом по теме» |

### LinkMethod
- `TitleSimilarity`, `Embedding`, `EntityOverlap`, `Hybrid`

### Файлы
- `TopicLinkClassifier`, `EmbeddingSimilarityCalculator`, `EntityOverlapCalculator`
- `OllamaEmbeddingService`, `NewsEmbeddingStore`
- `TopicLinkingService` — мульти-сигнальный пайплайн

### Docker
- `topic-linker` зависит от `ollama` + `ollama-embed-pull`
- Env: `TopicLinker__EmbeddingModel`, `TopicLinker__UseEmbeddings`, `TopicLinker__UseEntityOverlap`

## Деплой

```bash
docker compose up -d --build api frontend topic-linker
```

Первый запуск подтянет `nomic-embed-text`. Эмбеддинги создаются постепенно (до 40 за цикл).
