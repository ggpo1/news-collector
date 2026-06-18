# news_rewrites + CRUD API

## Таблица `news_rewrites`
| Поле | Тип | Описание |
|------|-----|----------|
| Id | uuid | PK |
| SourceNewsId | uuid | FK → `news` (исходная новость) |
| Title | varchar(1024) | Переписанный заголовок |
| Summary | text? | Краткое описание |
| Content | text? | Переписанный текст |
| CreatedAt, UpdatedAt | timestamptz | Метки времени |

При удалении исходной новости переписи удаляются каскадом.

## API ` /api/news-rewrites`
| Метод | Путь | Описание |
|-------|------|----------|
| GET | `?page&pageSize&sourceNewsId` | Список с пагинацией |
| GET | `/{id}` | Одна перепись |
| POST | `/` | Создать (тело: sourceNewsId, title, summary?, content?) |
| PUT | `/{id}` | Обновить (title, summary?, content?) |
| DELETE | `/{id}` | Удалить |

Миграция: `AddNewsRewrites`.

## Фронт (следующий шаг)
Кнопка «Переписать» → POST с текстом от LLM/редактора → сохранение через POST/PUT.

## Деплой
`docker compose build api worker && docker compose up -d` (миграции применяет worker).
