# Карточка темы (Story) + редакционные теги

**Дата:** 2026-06-20

## 1. Объект «Тема» (Story)

### Domain
- `Story` — заголовок, статус, clusterKey, счётчики, primary news
- `StoryNewsItem` — связь тема ↔ материалы
- `StoryStatus`: Monitoring, InWork, Published, Closed

### Sync
- `StorySyncService` — материализует кластеры из `news_links` (SameTopic/Duplicate/Related)
- Запускается в `TopicLinkerWorker` после каждого цикла linking

### API
- `GET /api/stories` — список с фильтром по статусу
- `GET /api/stories/{id}` — карточка темы
- `GET /api/stories/by-cluster/{clusterKey}` — для перехода с дашборда
- `PATCH /api/stories/{id}/status` — смена статуса
- `POST /api/stories/sync` — ручная синхронизация

### Карточка темы содержит
- Таймлайн материалов (RU + ино)
- Ключевые сущности
- Переписи по материалам темы
- Отправки в Telegram
- Селектор статуса

### Frontend
- Раздел **Темы** (`/stories`) в навигации
- Master-detail: список тем + карточка
- Из новости — ссылка «Открыть карточку темы»
- Из дашборда «Развивается тема» — переход в Story

## 2. Ручная категория + редакционные теги

### Domain
- `EditorialTag`, `NewsEditorialTag`
- `NewsItem.IsCategoryManual` — автокатегоризатор не перезаписывает

### Seed тегов
- #срочно, #needs-factcheck, #для-канала, #в-работе

### API
- `GET /api/editorial-tags`
- `PATCH /api/news/{id}/category`
- `PUT /api/news/{id}/editorial-tags`
- `GET /api/news?editorialTagId=`

### Frontend
- Панель «Редакционная разметка» в карточке новости
- Фильтр по редакционному тегу в ленте новостей
- Теги отображаются в списке новостей

## Деплой

```bash
docker compose up -d --build api frontend topic-linker
```

Миграция: `20260620190000_AddStoriesAndEditorialTags`
