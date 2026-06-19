# Пригласительные коды (invitation_codes)

## Суть
Регистрация только по одноразовому UUID-коду. Роль задаётся при создании кода главным редактором.

## Backend
- Таблица `invitation_codes`: `Code` (PK, uuid v4), `Role`, `CreatedAt`, `CreatedByUserId`, `UsedAt`, `UsedByUserId`
- `POST /api/auth/validate-invitation` — проверка кода (anonymous)
- `POST /api/auth/register` — регистрация с кодом, создание пользователя с ролью из кода, пометка кода использованным, авто-логин (JWT)
- `GET/POST /api/invitation-codes` — список и создание кодов (только ChiefEditor)
- Миграция: `AddInvitationCodes`

## Frontend
- На странице входа: «Зарегистрироваться по коду» → ввод кода → форма логин/имя/пароль
- В разделе «Пользователи»: создание кодов, список кодов, копирование

## Деплой
```bash
sudo docker compose up -d --build
```
