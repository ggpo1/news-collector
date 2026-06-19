# Fix: NewsRewrite.AuthorId EF configuration crash

## Проблема
API падал при старте на `MigrateAsync` с ошибкой:
```
The property 'NewsRewrite.AuthorId' cannot be marked as nullable/optional because the type of the property is 'Guid' which is not a nullable type.
```

## Причина
В `NewsRewriteConfiguration.cs` было `.IsRequired(false)` для свойства `Guid AuthorId`. EF Core не позволяет помечать non-nullable `Guid` как optional.

## Исправление
Удалён вызов `.IsRequired(false)` — для `Guid` по умолчанию свойство required.

Миграция оставлена с `nullable: true` для колонки `AuthorId` (legacy backfill через `AuthDataSeeder` после migrate).

## Деплой
```bash
sudo docker compose up -d --build
```
