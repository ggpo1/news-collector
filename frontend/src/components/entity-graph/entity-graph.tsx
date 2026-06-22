import { useEffect, useMemo, useRef, useState } from 'react';
import { ENTITY_TYPE_COLORS, ENTITY_TYPE_OPTIONS, entityTypeLabel } from '../../api/entity-labels';
import type { EntityGraph, EntityGraphNode, NamedEntityType } from '../../api/entities';
import { LoadingState } from '../ui/loading-state';
import * as S from './entity-graph.styles';

interface SimNode extends EntityGraphNode {
  x: number;
  y: number;
  vx: number;
  vy: number;
  radius: number;
}

interface SimLink {
  source: SimNode;
  target: SimNode;
  weight: number;
}

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

function buildSimulation(nodes: EntityGraphNode[], edges: EntityGraph['edges'], width: number, height: number) {
  const maxMentions = Math.max(...nodes.map((node) => node.mentionCount), 1);
  const simNodes: SimNode[] = nodes.map((node, index) => {
    const angle = (index / nodes.length) * Math.PI * 2;
    const spread = Math.min(width, height) * 0.3;
    return {
      ...node,
      x: width / 2 + Math.cos(angle) * spread,
      y: height / 2 + Math.sin(angle) * spread,
      vx: 0,
      vy: 0,
      radius: 6 + (node.mentionCount / maxMentions) * 14,
    };
  });

  const nodeById = new Map(simNodes.map((node) => [node.id, node]));
  const simLinks: SimLink[] = edges
    .map((edge) => {
      const source = nodeById.get(edge.sourceId);
      const target = nodeById.get(edge.targetId);
      if (!source || !target) {
        return null;
      }

      return { source, target, weight: edge.weight };
    })
    .filter((link): link is SimLink => link !== null);

  return { simNodes, simLinks, maxMentions };
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
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [hoveredId, setHoveredId] = useState<string | null>(null);
  const transformRef = useRef({ scale: 1, offsetX: 0, offsetY: 0 });
  const dragRef = useRef<{ mode: 'pan' | 'node'; nodeId?: string; lastX: number; lastY: number } | null>(null);
  const simRef = useRef<{ simNodes: SimNode[]; simLinks: SimLink[] } | null>(null);
  const sizeRef = useRef({ width: 800, height: 600 });

  const selectedNode = graph?.nodes.find((node) => node.id === selectedId) ?? null;
  const neighbors = useMemo(
    () => (graph && selectedId ? getNeighbors(selectedId, graph.edges, graph.nodes) : []),
    [graph, selectedId],
  );

  useEffect(() => {
    if (!graph || graph.nodes.length === 0) {
      simRef.current = null;
      return;
    }

    const { width, height } = sizeRef.current;
    simRef.current = buildSimulation(graph.nodes, graph.edges, width, height);
    transformRef.current = { scale: 1, offsetX: 0, offsetY: 0 };
    setSelectedId(null);
  }, [graph]);

  useEffect(() => {
    const wrap = wrapRef.current;
    const canvas = canvasRef.current;
    if (!wrap || !canvas) {
      return;
    }

    const resize = () => {
      const rect = wrap.getBoundingClientRect();
      const dpr = window.devicePixelRatio || 1;
      sizeRef.current = { width: rect.width, height: rect.height };
      canvas.width = Math.floor(rect.width * dpr);
      canvas.height = Math.floor(rect.height * dpr);
      canvas.style.width = `${rect.width}px`;
      canvas.style.height = `${rect.height}px`;

      if (graph && graph.nodes.length > 0) {
        simRef.current = buildSimulation(graph.nodes, graph.edges, rect.width, rect.height);
      }
    };

    resize();
    const observer = new ResizeObserver(resize);
    observer.observe(wrap);
    return () => observer.disconnect();
  }, [graph]);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) {
      return;
    }

    const ctx = canvas.getContext('2d');
    if (!ctx) {
      return;
    }

    let frameId = 0;
    let ticks = 0;

    const draw = () => {
      const dpr = window.devicePixelRatio || 1;
      const { width, height } = sizeRef.current;
      const sim = simRef.current;

      ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
      ctx.clearRect(0, 0, width, height);

      if (!sim) {
        frameId = requestAnimationFrame(draw);
        return;
      }

      if (ticks < 180) {
        const centerX = width / 2;
        const centerY = height / 2;

        for (const node of sim.simNodes) {
          for (const other of sim.simNodes) {
            if (node === other) {
              continue;
            }

            const dx = node.x - other.x;
            const dy = node.y - other.y;
            const dist = Math.max(Math.hypot(dx, dy), 1);
            const force = 900 / (dist * dist);
            node.vx += (dx / dist) * force;
            node.vy += (dy / dist) * force;
          }
        }

        for (const link of sim.simLinks) {
          const dx = link.target.x - link.source.x;
          const dy = link.target.y - link.source.y;
          const dist = Math.max(Math.hypot(dx, dy), 1);
          const force = dist * 0.004 * link.weight;
          link.source.vx += (dx / dist) * force;
          link.source.vy += (dy / dist) * force;
          link.target.vx -= (dx / dist) * force;
          link.target.vy -= (dy / dist) * force;
        }

        for (const node of sim.simNodes) {
          node.vx += (centerX - node.x) * 0.002;
          node.vy += (centerY - node.y) * 0.002;
          node.vx *= 0.84;
          node.vy *= 0.84;
          node.x += node.vx;
          node.y += node.vy;
        }

        ticks += 1;
      }

      const { scale, offsetX, offsetY } = transformRef.current;
      const activeId = hoveredId ?? selectedId;
      const highlightIds = new Set<string>();

      if (activeId && graph) {
        highlightIds.add(activeId);
        for (const edge of graph.edges) {
          if (edge.sourceId === activeId) {
            highlightIds.add(edge.targetId);
          } else if (edge.targetId === activeId) {
            highlightIds.add(edge.sourceId);
          }
        }
      }

      ctx.save();
      ctx.translate(offsetX, offsetY);
      ctx.scale(scale, scale);

      for (const link of sim.simLinks) {
        const isActive =
          activeId &&
          (link.source.id === activeId ||
            link.target.id === activeId ||
            (highlightIds.has(link.source.id) && highlightIds.has(link.target.id)));

        ctx.beginPath();
        ctx.moveTo(link.source.x, link.source.y);
        ctx.lineTo(link.target.x, link.target.y);
        ctx.strokeStyle = isActive
          ? 'rgba(91, 154, 255, 0.55)'
          : 'rgba(139, 156, 179, 0.18)';
        ctx.lineWidth = Math.max(1, Math.min(4, link.weight * 0.6));
        ctx.stroke();
      }

      for (const node of sim.simNodes) {
        const color = ENTITY_TYPE_COLORS[node.type];
        const isSelected = node.id === selectedId;
        const isHovered = node.id === hoveredId;
        const isDimmed = activeId !== null && !highlightIds.has(node.id);

        ctx.beginPath();
        ctx.arc(node.x, node.y, node.radius, 0, Math.PI * 2);
        ctx.fillStyle = isDimmed ? 'rgba(92, 109, 132, 0.45)' : color;
        ctx.fill();

        if (isSelected || isHovered) {
          ctx.strokeStyle = '#eef2f7';
          ctx.lineWidth = 2;
          ctx.stroke();
        }

        if (scale > 0.65 && node.radius >= 8) {
          ctx.fillStyle = isDimmed ? 'rgba(238, 242, 247, 0.55)' : '#eef2f7';
          ctx.font = `600 ${Math.max(10, node.radius * 0.75)}px IBM Plex Sans, sans-serif`;
          ctx.textAlign = 'center';
          ctx.textBaseline = 'top';
          const label = node.name.length > 22 ? `${node.name.slice(0, 20)}…` : node.name;
          ctx.fillText(label, node.x, node.y + node.radius + 4);
        }
      }

      ctx.restore();
      frameId = requestAnimationFrame(draw);
    };

    frameId = requestAnimationFrame(draw);
    return () => cancelAnimationFrame(frameId);
  }, [graph, hoveredId, selectedId]);

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

    for (let i = sim.simNodes.length - 1; i >= 0; i -= 1) {
      const node = sim.simNodes[i];
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
      setSelectedId(node.id);
      return;
    }

    dragRef.current = { mode: 'pan', lastX: event.clientX, lastY: event.clientY };
  };

  const handlePointerMove = (event: React.PointerEvent<HTMLCanvasElement>) => {
    const world = screenToWorld(event.clientX, event.clientY);
    const hovered = findNodeAt(world.x, world.y);
    setHoveredId(hovered?.id ?? null);

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
      return;
    }

    const sim = simRef.current;
    const node = sim?.simNodes.find((item) => item.id === drag.nodeId);
    if (!node) {
      return;
    }

    const { scale } = transformRef.current;
    node.x += dx / scale;
    node.y += dy / scale;
    node.vx = 0;
    node.vy = 0;
  };

  const handlePointerUp = (event: React.PointerEvent<HTMLCanvasElement>) => {
    dragRef.current = null;
    canvasRef.current?.releasePointerCapture(event.pointerId);
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
  };

  const isEmpty = !loading && graph && graph.nodes.length === 0;

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
              min={1}
              max={10}
              value={minWeight}
              onChange={(e) => onMinWeightChange(Number(e.target.value))}
            />
          </S.ControlGroup>

          <S.RefreshButton type="button" onClick={onReload} disabled={loading}>
            Обновить
          </S.RefreshButton>
        </S.ControlsBar>

        {error && <S.ErrorBanner role="alert">{error}</S.ErrorBanner>}

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
            onPointerLeave={handlePointerUp}
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
                  <S.NeighborItem key={node.id} type="button" onClick={() => setSelectedId(node.id)}>
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
