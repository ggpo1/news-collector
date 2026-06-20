# Категории новостей + NewsCategorizer worker

## БД

### `categories`
| Поле | Описание |
|------|----------|
| Id | uuid PK |
| Slug | уникальный код (`politics`, `economy`, …) |
| Name | отображаемое имя |
| Description | подсказка для AI-классификатора |
| IsActive | активна ли категория |
| SortOrder | порядок в списке |

Seed: politics, economy, society, world, tech, science, sports, culture, incidents, other.

### `news.CategoryId`
nullable FK → `categories`, ON DELETE SET NULL.

Индекс `(FetchedAt) WHERE CategoryId IS NULL` — для батч-выборки воркером.

## Worker `NewsCollector.NewsCategorizer`
- Каждые 60 с берёт до 10 новостей без категории (`CategoryId IS NULL`)
- Ollama классифицирует по заголовку/summary/content
- Пишет `category_id`; при ошибке — fallback `other`

## Конфиг (`NewsCategorizer`)
| Параметр | Default |
|----------|---------|
| PollingIntervalSeconds | 60 |
| BatchSize | 10 |
| DelayBetweenItemsMs | 200 |

Ollama: те же `OLLAMA_*`, что у API.

## Деплой
```bash
docker compose up -d --build worker news-categorizer
```

Миграция: `AddCategories` (применяет worker/api/news-categorizer при `RunMigrations=true`).

## SQL
```sql
SELECT c."Name", COUNT(*) FROM news n
JOIN categories c ON c."Id" = n."CategoryId"
GROUP BY c."Name" ORDER BY COUNT(*) DESC;
```
