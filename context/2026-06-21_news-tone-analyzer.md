# News tone analyzer (коэффициент тональности)

## Суть
Отдельный worker `NewsCollector.NewsToneAnalyzer` батчем оценивает тональность новостей через Ollama.

## Поля в `news`
| Поле | Тип | Описание |
|------|-----|----------|
| ToneCoefficient | numeric(4,3)? | от -1 (негатив) до +1 (позитив) |
| ToneAnalyzedAt | timestamptz? | когда посчитано |

## Worker
- Каждые 60 с берёт до 10 новостей где `ToneCoefficient IS NULL`
- Приоритет: свежие (`PublishedAt` → `FetchedAt` → `CreatedAt`)
- Ollama возвращает `{"toneCoefficient":0.42}`

## Конфиг (`NewsToneAnalyzer`)
| Параметр | Default |
|----------|---------|
| PollingIntervalSeconds | 60 |
| BatchSize | 10 |
| DelayBetweenItemsMs | 200 |

## Деплой
```bash
docker compose up -d --build news-tone-analyzer api frontend
```

Миграция: `AddNewsToneCoefficient`.

## API
`toneCoefficient` в `NewsItemListDto` / `NewsItemDetailDto`.

## SQL
```sql
SELECT AVG("ToneCoefficient"), COUNT(*) FROM news WHERE "ToneCoefficient" IS NOT NULL;
```
