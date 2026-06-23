# 2026-06-20: Оптимизация производительности карты сущностей

## Проблема
Страница `/map` лагала при ~150 узлах и 300+ рёбрах (данные за 7 дней, minWeight=2).

## Причины
1. Бесконечный `requestAnimationFrame` — перерисовка 60 fps даже в покое.
2. Force-layout (O(n²) отталкивание) выполнялся **внутри** цикла отрисовки на каждом кадре (~180 тиков).
3. Подсветка соседей — полный проход по всем рёбрам каждый кадр.
4. Подписи ко всем узлам при большом графе.
5. API отдавал до `maxNodes * 3` рёбер (450 при 150 узлах).

## Исправления

### Frontend (`entity-graph.tsx`, `entity-graph-layout.ts`)
- Layout один раз при загрузке/resize, затем статичные координаты.
- Отрисовка только по событию (hover, pan, zoom, drag) через `scheduleDraw` с дедупликацией rAF.
- Предвычисленная `neighborMap` для подсветки.
- Подписи: все узлы ≤35, иначе топ-20 по mentionCount + выбранный/hover.
- DPR ограничен до 2.
- Подсказка при плотном графе.

### Дефолты UI (`entity-map-page.tsx`)
- `maxNodes`: 150 → **60**
- `minWeight`: 2 → **5**

### Backend
- `NewsEntityGraphQueryService`: лимит рёбер `min(maxNodes * 2, 100)`, maxNodes clamp 120.
- `EntitiesController`: дефолты `maxNodes=60`, `minWeight=3`, валидация maxNodes ≤120.

## Hotfix: прыжки при hover
`scheduleDraw` зависел от `hoveredId` → менялся `rebuildSimulation` → `useEffect` перезапускал layout на каждый hover.

Исправление: hover/selection для canvas через refs, `scheduleDraw` со стабильными deps `[]`, layout только при смене `graph` или resize.
