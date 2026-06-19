# CRUD источников (sources)

## API
- `ISourcesService` / `SourcesService` — полный CRUD вместо read-only `ISourcesQueryService`
- `POST /api/sources` — создание
- `PUT /api/sources/{id}` — обновление
- `DELETE /api/sources/{id}` — удаление (409, если есть новости)
- `SourceDto` расширен: `contentFetchEnabled`, `contentSelector` (из jsonb config)
- DTO запросов: `CreateSourceRequest`, `UpdateSourceRequest`

## Frontend
- Вкладка **«Источники»** в навигации
- Список источников + кнопка «Добавить источник»
- Модальная форма `source-form`: название, тип, URL, интервал, активность, загрузка полного текста, CSS-селектор
- Редактирование и удаление из списка
- `useSources` возвращает все источники + `reload()`; в разделе «Новости» фильтруются только активные

## Деплой
```bash
sudo docker compose up -d --build
```
