import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { ENTITY_TYPE_COLORS, ENTITY_TYPE_OPTIONS, entityTypeLabel } from '../../api/entity-labels';
import type { EntityGraph, EntityGraphNode, NamedEntityType } from '../../api/entities';
import { LoadingState } from '../ui/loading-state';
import {
  buildSimulation,
  runLayout,
  type GraphSimulation,
  type SimNode,
} from './entity-graph-layout';
import * as S from './entity-graph.styles';

interface EntityGraphViewProps {
  graph: EntityGraph | null;
  loading: boolean;
  error: string | null;
  period: '1d' | '7d' | '30d';
  entityType: NamedEntityType | '';
  minWeight: number;
  onPeriodChange: (period: '1d' | '7d' | '30d') => void;
  onEntityTypeChange: (type: NamedEntityType | '') => void;
  onMinWeightChange: (weight: number) => void;
  onReload: () => void;
}

function getNeighbors(
  nodeId: string,
  edges: EntityGraph['edges'],
  nodes: EntityGraphNode[],
): { node: EntityGraphNode; weight: number }[] {
  const nodeById = new Map(nodes.map((node) => [node.id, node]));
  const neighbors = new Map<string, number>();

  for (const edge of edges) {
    if (edge.sourceId === nodeId) {
      neighbors.set(edge.targetId, edge.weight);
    } else if (edge.targetId === nodeId) {
      neighbors.set(edge.sourceId, edge.weight);
    }
  }

  return [...neighbors.entries()]
    .map(([id, weight]) => {
      const node = nodeById.get(id);
      return node ? { node, weight } : null;
    })
    .filter((item): item is { node: EntityGraphNode; weight: number } => item !== null)
    .sort((a, b) => b.weight - a.weight);
}

function buildHighlightSet(
  activeId: string | null,
  neighborMap: Map<string, Set<string>> | null,
): Set<string> | null {
  if (!activeId || !neighborMap) {
    return null;
  }

  const highlights = new Set<string>([activeId]);
  const neighbors = neighborMap.get(activeId);
  if (neighbors) {
    for (const id of neighbors) {
      highlights.add(id);
    }
  }

  return highlights;
}

export function EntityGraphView({
  graph,
  loading,
  error,
  period,
  entityType,
  minWeight,
  onPeriodChange,
  onEntityTypeChange,
  onMinWeightChange,
  onReload,
}: EntityGraphViewProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const wrapRef = useRef<HTMLDivElement>(null);
  const simRef = useRef<GraphSimulation | null>(null);
  const sizeRef = useRef({ width: 800, height: 600 });
  const transformRef = useRef({ scale: 1, offsetX: 0, offsetY: 0 });
  const dragRef = useRef<{ mode: 'pan' | 'node'; nodeId?: string; lastX: number; lastY: number } | null>(null);
  const rafRef = useRef<number | null>(null);
  const hoveredIdRef = useRef<string | null>(null);
  const selectedIdRef = useRef<string | null>(null);
  const labelNodeIdsRef = useRef<Set<string>>(new Set());

  const [selectedId, setSelectedId] = useState<string | null>(null);

  const selectedNode = graph?.nodes.find((node) => node.id === selectedId) ?? null;
  const neighbors = useMemo(
    () => (graph && selectedId ? getNeighbors(selectedId, graph.edges, graph.nodes) : []),
    [graph, selectedId],
  );

  const labelNodeIds = useMemo(() => {
    if (!graph) {
      return new Set<string>();
    }

    if (graph.nodes.length <= 35) {
      return new Set(graph.nodes.map((node) => node.id));
    }

    return new Set(
      [...graph.nodes]
        .sort((a, b) => b.mentionCount - a.mentionCount)
        .slice(0, 20)
        .map((node) => node.id),
    );
  }, [graph]);

  const scheduleDraw = useCallback(() => {
    if (rafRef.current !== null) {
      return;
    }

    rafRef.current = requestAnimationFrame(() => {
      rafRef.current = null;
      const canvas = canvasRef.current;
      const sim = simRef.current;
      if (!canvas || !sim) {
        return;
      }

      const ctx = canvas.getContext('2d');
      if (!ctx) {
        return;
      }

      const dpr = Math.min(window.devicePixelRatio || 1, 2);
      const { width, height } = sizeRef.current;
      const hoveredId = hoveredIdRef.current;
      const selectedId = selectedIdRef.current;
      const labelNodeIds = labelNodeIdsRef.current;
      const activeId = hoveredId ?? selectedId;
      const highlightIds = buildHighlightSet(activeId, sim.neighborMap);
      const showLabels = sim.nodes.length <= 50;

      ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
      ctx.clearRect(0, 0, width, height);

      const { scale, offsetX, offsetY } = transformRef.current;
      ctx.save();
      ctx.translate(offsetX, offsetY);
      ctx.scale(scale, scale);

      ctx.lineCap = 'round';
      for (const link of sim.links) {
        const isActive =
          activeId &&
          (link.source.id === activeId ||
            link.target.id === activeId ||
            (highlightIds?.has(link.source.id) && highlightIds.has(link.target.id)));

        ctx.beginPath();
        ctx.moveTo(link.source.x, link.source.y);
        ctx.lineTo(link.target.x, link.target.y);
        ctx.strokeStyle = isActive
          ? 'rgba(91, 154, 255, 0.55)'
          : 'rgba(139, 156, 179, 0.16)';
        ctx.lineWidth = isActive
          ? Math.max(1, Math.min(3.5, link.weight * 0.45))
          : Math.max(0.6, Math.min(2.5, link.weight * 0.35));
        ctx.stroke();
      }

      for (const node of sim.nodes) {
        const color = ENTITY_TYPE_COLORS[node.type];
        const isSelected = node.id === selectedId;
        const isHovered = node.id === hoveredId;
        const isDimmed = highlightIds !== null && !highlightIds.has(node.id);

        ctx.beginPath();
        ctx.arc(node.x, node.y, node.radius, 0, Math.PI * 2);
        ctx.fillStyle = isDimmed ? 'rgba(92, 109, 132, 0.4)' : color;
        ctx.fill();

        if (isSelected || isHovered) {
          ctx.strokeStyle = '#eef2f7';
          ctx.lineWidth = 2;
          ctx.stroke();
        }

        const shouldLabel =
          showLabels &&
          scale > 0.55 &&
          (labelNodeIds.has(node.id) || isSelected || isHovered);

        if (shouldLabel) {
          ctx.fillStyle = isDimmed ? 'rgba(238, 242, 247, 0.55)' : '#eef2f7';
          ctx.font = `600 ${Math.max(9, node.radius * 0.72)}px IBM Plex Sans, sans-serif`;
          ctx.textAlign = 'center';
          ctx.textBaseline = 'top';
          const label = node.name.length > 18 ? `${node.name.slice(0, 16)}…` : node.name;
          ctx.fillText(label, node.x, node.y + node.radius + 3);
        }
      }

      ctx.restore();
    });
  }, []);

  const layoutSimulation = useCallback(() => {
    if (!graph || graph.nodes.length === 0) {
      simRef.current = null;
      return;
    }

    const { width, height } = sizeRef.current;
    const simulation = buildSimulation(graph.nodes, graph.edges, width, height);
    runLayout(simulation, width, height);
    simRef.current = simulation;
  }, [graph]);

  useEffect(() => {
    selectedIdRef.current = null;
    hoveredIdRef.current = null;
    labelNodeIdsRef.current = labelNodeIds;
    setSelectedId(null);
    layoutSimulation();
    transformRef.current = { scale: 1, offsetX: 0, offsetY: 0 };
    scheduleDraw();
  }, [graph, labelNodeIds, layoutSimulation, scheduleDraw]);

  useEffect(() => {
    selectedIdRef.current = selectedId;
    scheduleDraw();
  }, [selectedId, scheduleDraw]);

  useEffect(() => {
    const wrap = wrapRef.current;
    const canvas = canvasRef.current;
    if (!wrap || !canvas) {
      return;
    }

    const resize = () => {
      const rect = wrap.getBoundingClientRect();
      const dpr = Math.min(window.devicePixelRatio || 1, 2);
      sizeRef.current = { width: rect.width, height: rect.height };
      canvas.width = Math.floor(rect.width * dpr);
      canvas.height = Math.floor(rect.height * dpr);
      canvas.style.width = `${rect.width}px`;
      canvas.style.height = `${rect.height}px`;
      layoutSimulation();
      scheduleDraw();
    };

    resize();
    const observer = new ResizeObserver(resize);
    observer.observe(wrap);

    return () => {
      observer.disconnect();
      if (rafRef.current !== null) {
        cancelAnimationFrame(rafRef.current);
        rafRef.current = null;
      }
    };
  }, [layoutSimulation, scheduleDraw]);

  const screenToWorld = (clientX: number, clientY: number) => {
    const rect = canvasRef.current?.getBoundingClientRect();
    if (!rect) {
      return { x: 0, y: 0 };
    }

    const { scale, offsetX, offsetY } = transformRef.current;
    return {
      x: (clientX - rect.left - offsetX) / scale,
      y: (clientY - rect.top - offsetY) / scale,
    };
  };

  const findNodeAt = (x: number, y: number): SimNode | null => {
    const sim = simRef.current;
    if (!sim) {
      return null;
    }

    for (let i = sim.nodes.length - 1; i >= 0; i -= 1) {
      const node = sim.nodes[i];
      const dist = Math.hypot(node.x - x, node.y - y);
      if (dist <= node.radius + 4) {
        return node;
      }
    }

    return null;
  };

  const handlePointerDown = (event: React.PointerEvent<HTMLCanvasElement>) => {
    const world = screenToWorld(event.clientX, event.clientY);
    const node = findNodeAt(world.x, world.y);
    canvasRef.current?.setPointerCapture(event.pointerId);

    if (node) {
      dragRef.current = { mode: 'node', nodeId: node.id, lastX: event.clientX, lastY: event.clientY };
      selectedIdRef.current = node.id;
      setSelectedId(node.id);
      return;
    }

    dragRef.current = { mode: 'pan', lastX: event.clientX, lastY: event.clientY };
  };

  const handlePointerMove = (event: React.PointerEvent<HTMLCanvasElement>) => {
    const world = screenToWorld(event.clientX, event.clientY);
    const hovered = findNodeAt(world.x, world.y);
    const nextHoveredId = hovered?.id ?? null;
    if (nextHoveredId !== hoveredIdRef.current) {
      hoveredIdRef.current = nextHoveredId;
      scheduleDraw();
    }

    const drag = dragRef.current;
    if (!drag) {
      return;
    }

    const dx = event.clientX - drag.lastX;
    const dy = event.clientY - drag.lastY;
    drag.lastX = event.clientX;
    drag.lastY = event.clientY;

    if (drag.mode === 'pan') {
      transformRef.current.offsetX += dx;
      transformRef.current.offsetY += dy;
      scheduleDraw();
      return;
    }

    const sim = simRef.current;
    const node = sim?.nodes.find((item) => item.id === drag.nodeId);
    if (!node) {
      return;
    }

    const { scale } = transformRef.current;
    node.x += dx / scale;
    node.y += dy / scale;
    scheduleDraw();
  };

  const handlePointerUp = (event: React.PointerEvent<HTMLCanvasElement>) => {
    dragRef.current = null;
    canvasRef.current?.releasePointerCapture(event.pointerId);
  };

  const handlePointerLeave = (event: React.PointerEvent<HTMLCanvasElement>) => {
    if (hoveredIdRef.current !== null) {
      hoveredIdRef.current = null;
      scheduleDraw();
    }
    handlePointerUp(event);
  };

  const handleWheel = (event: React.WheelEvent<HTMLCanvasElement>) => {
    event.preventDefault();
    const rect = canvasRef.current?.getBoundingClientRect();
    if (!rect) {
      return;
    }

    const transform = transformRef.current;
    const mouseX = event.clientX - rect.left;
    const mouseY = event.clientY - rect.top;
    const zoom = event.deltaY < 0 ? 1.08 : 0.92;
    const nextScale = Math.min(3, Math.max(0.35, transform.scale * zoom));
    const scaleRatio = nextScale / transform.scale;

    transform.offsetX = mouseX - (mouseX - transform.offsetX) * scaleRatio;
    transform.offsetY = mouseY - (mouseY - transform.offsetY) * scaleRatio;
    transform.scale = nextScale;
    scheduleDraw();
  };

  const isEmpty = !loading && graph && graph.nodes.length === 0;
  const isDense = graph && (graph.nodes.length > 80 || graph.edges.length > 120);

  return (
    <S.GraphLayout>
      <S.GraphMain>
        <S.ControlsBar>
          <S.ControlGroup>
            Период
            <S.Select value={period} onChange={(e) => onPeriodChange(e.target.value as '1d' | '7d' | '30d')}>
              <option value="1d">24 часа</option>
              <option value="7d">7 дней</option>
              <option value="30d">30 дней</option>
            </S.Select>
          </S.ControlGroup>

          <S.ControlGroup>
            Тип
            <S.Select
              value={entityType}
              onChange={(e) => onEntityTypeChange(e.target.value as NamedEntityType | '')}
            >
              {ENTITY_TYPE_OPTIONS.map((option) => (
                <option key={option.label} value={option.value}>
                  {option.label}
                </option>
              ))}
            </S.Select>
          </S.ControlGroup>

          <S.ControlGroup>
            Мин. связь
            <S.RangeValue as="div">{minWeight}</S.RangeValue>
            <S.RangeInput
              type="range"
              min={2}
              max={15}
              value={minWeight}
              onChange={(e) => onMinWeightChange(Number(e.target.value))}
            />
          </S.ControlGroup>

          <S.RefreshButton type="button" onClick={onReload} disabled={loading}>
            Обновить
          </S.RefreshButton>
        </S.ControlsBar>

        {error && <S.ErrorBanner role="alert">{error}</S.ErrorBanner>}
        {isDense && !loading && (
          <S.DenseHint>
            Граф плотный — увеличьте «Мин. связь» или сузьте период для более читаемой карты.
          </S.DenseHint>
        )}

        <S.CanvasWrap ref={wrapRef}>
          {loading && (
            <S.LoadingOverlay>
              <LoadingState label="Строим граф связей…" />
            </S.LoadingOverlay>
          )}
          {!loading && isEmpty && (
            <S.EmptyOverlay>
              Нет данных за выбранный период. Подождите, пока entity-extractor обработает новости, или уменьшите
              «Мин. связь».
            </S.EmptyOverlay>
          )}
          <S.Canvas
            ref={canvasRef}
            onPointerDown={handlePointerDown}
            onPointerMove={handlePointerMove}
            onPointerUp={handlePointerUp}
            onPointerLeave={handlePointerLeave}
            onWheel={handleWheel}
          />
          <S.GraphHint>Колёсико — масштаб · перетаскивание — панорама · клик — детали</S.GraphHint>
        </S.CanvasWrap>

        {graph && (
          <S.StatsBar>
            <span>Узлов: {graph.nodes.length}</span>
            <span>Связей: {graph.edges.length}</span>
            <span>
              Период: {new Date(graph.from).toLocaleDateString('ru-RU')} —{' '}
              {new Date(graph.to).toLocaleDateString('ru-RU')}
            </span>
          </S.StatsBar>
        )}

        <S.Legend>
          {(Object.keys(ENTITY_TYPE_COLORS) as NamedEntityType[]).map((type) => (
            <S.LegendItem key={type} $color={ENTITY_TYPE_COLORS[type]}>
              {entityTypeLabel(type)}
            </S.LegendItem>
          ))}
        </S.Legend>
      </S.GraphMain>

      <S.DetailPanel $open={Boolean(selectedNode)}>
        {selectedNode ? (
          <>
            <S.DetailHeader>
              <S.DetailTitle>{selectedNode.name}</S.DetailTitle>
              <S.DetailMeta>
                <S.TypeBadge $color={ENTITY_TYPE_COLORS[selectedNode.type]}>
                  {entityTypeLabel(selectedNode.type)}
                </S.TypeBadge>
                <span>Упоминаний: {selectedNode.mentionCount}</span>
              </S.DetailMeta>
            </S.DetailHeader>
            <S.NeighborsList>
              {neighbors.length === 0 ? (
                <S.EmptyOverlay style={{ position: 'static', padding: '1rem' }}>
                  Нет связей в текущем графе
                </S.EmptyOverlay>
              ) : (
                neighbors.map(({ node, weight }) => (
                  <S.NeighborItem
                    key={node.id}
                    type="button"
                    onClick={() => {
                      selectedIdRef.current = node.id;
                      setSelectedId(node.id);
                    }}
                  >
                    <S.NeighborName>{node.name}</S.NeighborName>
                    <S.NeighborWeight>{weight}</S.NeighborWeight>
                  </S.NeighborItem>
                ))
              )}
            </S.NeighborsList>
          </>
        ) : (
          <S.EmptyOverlay style={{ position: 'static', minHeight: '8rem' }}>
            Выберите узел на графе, чтобы увидеть соседей по совместным упоминаниям
          </S.EmptyOverlay>
        )}
      </S.DetailPanel>
    </S.GraphLayout>
  );
}
