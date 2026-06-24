# Редакционный дашборд «Что важно сейчас»

**Дата:** 2026-06-20

## Задача

Приоритет 1 для редакторов: не листать 200 RSS, а видеть 10–15 поводов:
- развивающиеся темы (3+ источника за окно);
- всплески сущностей vs предыдущий период;
- дубликаты с «лучшим» первичным материалом;
- эмоционально окрашенные материалы.

## Backend

- `EditorialDashboardOptions` — окно 48ч, min 3 источника, пороги spike.
- `EditorialDashboardService` — кластеризация через `news_links` (SameTopic/Duplicate), union-find в `EditorialNewsClusterBuilder`.
- Выбор primary: полный текст + раньше по `PublishedAt`/`FetchedAt`.
- `GET /api/editorial/dashboard?windowHours=24|48|72`
- Регистрация в `DependencyInjection.cs`.

## Frontend

- Страница `/dashboard` — стартовая после логина (`/` → `/dashboard`).
- Навигация: пункт «Обзор».
- Компонент `EditorialDashboardView` — 4 секции, переключатель окна, refresh.
- Hook `useEditorialDashboard`.

## Зависимости данных

- **Развивающиеся темы / дубликаты:** нужен `topic-linker` (заполнение `news_links`).
- **Всплески сущностей:** нужен `news-entity-extractor`.
- **Тональность:** нужен `news-tone-analyzer`.

## Деплой

```bash
docker compose up -d --build api frontend
```

API endpoint: `GET /api/editorial/dashboard`
