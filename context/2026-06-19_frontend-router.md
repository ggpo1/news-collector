# Frontend: react-router (разделы в URL)

## Маршруты
- `/login`, `/register`, `/register/details` — гостевые
- `/news`, `/links`, `/rewrites`, `/sources`, `/users` — под `RequireAuth`
- `/` → редирект на `/news`
- Chief-only: `/sources`, `/users` через `ChiefEditorRoute`

## Поведение
- F5 сохраняет текущий раздел
- После логина — возврат на `state.from` или `/news`
- Выход → `/login`
- Переход из переписей к новости: `navigate('/news', { state: { sourceId, newsId } })`

## Зависимость
`react-router-dom`

## Следующий этап (по запросу)
Глубокие URL: `/news/:sourceId/:newsId?page=2`
