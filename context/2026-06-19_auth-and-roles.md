# Авторизация и роли редакторов

## Роли
- **ChiefEditor** (главный редактор) — все переписи, управление источниками и пользователями
- **Editor** (обычный редактор) — только свои переписи, чтение новостей и связей

## Backend
- Таблица `users`, JWT Bearer auth
- `news_rewrites.AuthorId` → FK на пользователя
- Уникальный индекс `(SourceNewsId, AuthorId)` — одна перепись на новость от редактора
- `POST /api/auth/login`, `GET /api/auth/me`
- `GET/POST/PUT /api/users` — только ChiefEditor
- Источники: GET — все авторизованные; POST/PUT/DELETE — ChiefEditor
- Переписи: Editor видит/редактирует только свои; ChiefEditor — все

## Конфиг (.env)
```
AUTH_JWT_SECRET=...минимум 32 символа...
AUTH_SEED_CHIEF_LOGIN=chief
AUTH_SEED_CHIEF_PASSWORD=changeme
```

При первом запуске создаётся главный редактор. Старые переписи привязываются к нему.

## Frontend
- Страница входа, JWT в localStorage
- Вкладки «Источники» и «Пользователи» — только для главного редактора
- В списке переписей у chief отображается автор

## Деплой
```bash
sudo docker compose up -d --build
```
