# Frontend: раздел «Связи»

## API
- `GET /api/news-links?page&pageSize&linkType` — пагинированный список связей с обеими новостями
- `GET /api/news/{id}/related` — связи для одной новости (блок в карточке)

## UI
- Вкладки **Новости** | **Связи** (`app-nav`)
- Раздел **Связи**: фильтр по типу, список пар, детальная панель с уверенностью и ссылками
- В карточке новости — блок «Связанные новости»

## Компоненты
- `news-links-view`, `news-links-list`, `news-link-detail`, `link-type-filter`, `app-nav`

## Деплой
`docker compose build api frontend && docker compose up -d api frontend`
