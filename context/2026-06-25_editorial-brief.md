# Редакционный бриф (утро / вечер)

**Дата:** 2026-06-25

## Задача
Авто-сводка для chief editor: главные темы, новое vs прошлый бриф, уже отправленное в Telegram, дыры в покрытии RU/запад.

## Backend
- Таблица `editorial_brief_reports` — сохранённый markdown
- `EditorialBriefContextBuilder` — агрегация из dashboard, stories, telegram deliveries, coverage gaps
- `EditorialBriefGenerator` — LLM (Ollama `/api/chat`, markdown без JSON)
- `EditorialBriefService` — CRUD + расписание
- Воркер `NewsCollector.EditorialBrief` — проверка каждые N сек, генерация в UTC 6:00 и 18:00

## API
- `GET /api/editorial/brief?period=Morning|Evening` — последний бриф (404 если нет)
- `GET /api/editorial/brief/history?limit=14`
- `POST /api/editorial/brief/generate?period=Morning|Evening` — chief editor only

## Frontend
- Раздел **Бриф** (`/brief`) в навигации
- Фильтр: последний / утренний / вечерний
- Кнопка «Сгенерировать сейчас» — только chief editor

## Конфиг (`EditorialBrief`)
| Параметр | Default | Описание |
|----------|---------|----------|
| PollingIntervalSeconds | 3600 | Интервал проверки воркера |
| ScheduleHoursUtc | [6, 18] | Часы UTC для утра/вечера |
| WindowHours | 12 | Окно агрегации данных |
| RuSourcePatterns / WesternSourcePatterns | списки доменов | Эвристика для coverage gaps |

## Деплой
```bash
docker compose up -d --build api frontend editorial-brief
```

Миграция: `20260625003512_AddEditorialBriefReports` (применяется через `api` при `RunMigrations=true`).
