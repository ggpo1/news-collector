# Fix: /promo недоступен

## Симптом
`/promo` не открывался ни для гостя, ни для залогиненного пользователя — редирект на `/login` или `/news`.

## Причины
1. **Catch-all** `path="*"` → `Navigate to="/news"` — при старом бандле без маршрута `/promo` любой заход на promo попадал в `/news` → `RequireAuth` → login.
2. **Layout `path="/"`** — родительский маршрут с вложенными `dashboard`, `news` и т.д. потенциально конкурировал с публичными URL.
3. **Порт** — фронт на `:5190`, API на `:5182`; `/promo` на API не существует.
4. **PromoLink** на login — `styled(LinkButton) as={Link}`; заменён на `styled(Link)`.

## Исправление
- `app.tsx`: pathless layout для авторизованных разделов, абсолютные пути (`PATHS.dashboard` и т.д.).
- `root-redirect.tsx`: `/` и `*` → promo (гость) или dashboard (авторизован).
- `login-page.styles.ts`: `PromoLink = styled(Link)`.

## Деплой
```bash
docker compose up -d --build frontend
```
Открывать: `http://<host>:5190/promo` (не порт API).
