# Topic linker worker

## Задача
Отдельный воркер `NewsCollector.TopicLinker` раз в минуту ищет новости на одну тему и пишет связи в `news_links`.

## Алгоритм
1. Загружает до 300 свежих новостей за последние 168 ч (настраивается).
2. Попарно сравнивает заголовки (+ summary, если есть) через **Jaccard** по значимым токенам.
3. Стоп-слова (RU/EN) и короткие токены отфильтрованы.
4. Если `similarity >= 0.45` и `>= 2` общих токена — создаётся связь:
   - `LinkType.SameTopic` (или `Duplicate` при similarity >= 0.92)
   - `LinkMethod.TitleSimilarity`
   - `Confidence` = similarity score
5. Пары канонизируются: `NewsIdLow < NewsIdHigh` (CHECK в БД).

## Проекты
- `NewsCollector.TopicLinker` — hosted service, интервал 60 с
- `TopicLinkingService` + `TitleSimilarityCalculator` в Infrastructure
- `TopicLinkerOptions` в Application

## Docker
```bash
docker compose build topic-linker && docker compose up -d topic-linker
```

Сервис `topic-linker` в `docker-compose.yml`, образ `Dockerfile.topic-linker`.

## Конфиг (`TopicLinker` в appsettings)
| Параметр | Default | Описание |
|----------|---------|----------|
| PollingIntervalSeconds | 60 | Интервал цикла |
| LookbackHours | 168 | Окно поиска |
| MaxCandidates | 300 | Лимит новостей за цикл |
| MinSimilarity | 0.45 | Порог Jaccard |
| MinSharedTokens | 2 | Мин. общих слов |
| DuplicateSimilarity | 0.92 | Порог для Duplicate |

## API
Связанные новости уже отдаются `GET /api/news/{id}/related`.

## Локально
```bash
dotnet run --project src/NewsCollector.TopicLinker
```
