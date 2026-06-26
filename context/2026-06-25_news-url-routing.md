# Shareable URL для ленты новостей

**Дата:** 2026-06-25

## Формат
`/news/<page>?sourceId=&categoryId=&tone=&newsId=`

- `categoryId=__uncategorized__` — без категории
- `tone`: positive | negative | neutral | strong | unanalyzed
- `newsId` — открытая карточка новости (опционально)

`/news` → редирект на `/news/1` с сохранением query.

## Файлы
- `frontend/src/app/news-route.ts` — parse/build
- `frontend/src/app/news-root-redirect.tsx`
- `frontend/src/pages/news-page.tsx` — синхронизация фильтров с URL
- `app.tsx` — маршруты `/news` и `/news/:pageNum`

Редакционный тег пока только в локальном state (не в URL).
