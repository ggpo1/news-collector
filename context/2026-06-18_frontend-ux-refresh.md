# UX refresh: responsive app shell

## Что сделано

### Дизайн-система
- Расширен `theme.ts`: breakpoints, spacing, shadows, layout tokens, transitions
- Градиентный фон, улучшенные focus/selection стили
- `media.ts` + хук `useMediaQuery`

### App shell
- **Desktop (≥1024px)**: боковая панель с навигацией и брендингом
- **Mobile/Tablet**: sticky top bar + **нижняя tab-навигация** с иконками
- Safe area insets для iPhone (viewport-fit=cover)

### Master-detail (mobile-first)
- Компонент `MasterDetailLayout`
- На мобильных: список → полноэкранная деталь с кнопкой «Назад»
- На десктопе: две колонки, sticky detail panel
- Применено к: Новости, Связи, Переписи

### UI-паттерны
- `EmptyState`, `LoadingState` со спиннером
- Карточки списков: chevron на мобильных, крупные touch targets (min 44px)
- Модалки: bottom sheet на мобильных, centered dialog на desktop
- Источники: responsive grid (1/2/3 колонки)
- Пагинация: full-width кнопки на узких экранах

## Деплой
```bash
sudo docker compose up -d --build frontend
```
