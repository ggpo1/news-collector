import type { EntityGraph, EntityGraphNode } from '../../api/entities';

export interface SimNode extends EntityGraphNode {
  x: number;
  y: number;
  vx: number;
  vy: number;
  radius: number;
}

export interface SimLink {
  source: SimNode;
  target: SimNode;
  weight: number;
}

export interface GraphSimulation {
  nodes: SimNode[];
  links: SimLink[];
  neighborMap: Map<string, Set<string>>;
}

export function buildSimulation(
  nodes: EntityGraphNode[],
  edges: EntityGraph['edges'],
  width: number,
  height: number,
): GraphSimulation {
  const maxMentions = Math.max(...nodes.map((node) => node.mentionCount), 1);
  const simNodes: SimNode[] = nodes.map((node, index) => {
    const angle = (index / nodes.length) * Math.PI * 2;
    const spread = Math.min(width, height) * 0.32;
    return {
      ...node,
      x: width / 2 + Math.cos(angle) * spread,
      y: height / 2 + Math.sin(angle) * spread,
      vx: 0,
      vy: 0,
      radius: 5 + (node.mentionCount / maxMentions) * 12,
    };
  });

  const nodeById = new Map(simNodes.map((node) => [node.id, node]));
  const links: SimLink[] = edges
    .map((edge) => {
      const source = nodeById.get(edge.sourceId);
      const target = nodeById.get(edge.targetId);
      if (!source || !target) {
        return null;
      }

      return { source, target, weight: edge.weight };
    })
    .filter((link): link is SimLink => link !== null);

  const neighborMap = new Map<string, Set<string>>();
  for (const link of links) {
    if (!neighborMap.has(link.source.id)) {
      neighborMap.set(link.source.id, new Set());
    }

    if (!neighborMap.has(link.target.id)) {
      neighborMap.set(link.target.id, new Set());
    }

    neighborMap.get(link.source.id)!.add(link.target.id);
    neighborMap.get(link.target.id)!.add(link.source.id);
  }

  return { nodes: simNodes, links, neighborMap };
}

export function runLayout(simulation: GraphSimulation, width: number, height: number): void {
  const { nodes, links } = simulation;
  const centerX = width / 2;
  const centerY = height / 2;
  const nodeCount = nodes.length;
  const iterations = nodeCount > 80 ? 50 : nodeCount > 40 ? 70 : 90;
  const repulsionStrength = nodeCount > 80 ? 520 : 760;

  for (let tick = 0; tick < iterations; tick += 1) {
    for (let i = 0; i < nodeCount; i += 1) {
      const node = nodes[i];
      for (let j = i + 1; j < nodeCount; j += 1) {
        const other = nodes[j];
        const dx = node.x - other.x;
        const dy = node.y - other.y;
        const distSq = Math.max(dx * dx + dy * dy, 64);
        const force = repulsionStrength / distSq;
        const dist = Math.sqrt(distSq);
        const fx = (dx / dist) * force;
        const fy = (dy / dist) * force;
        node.vx += fx;
        node.vy += fy;
        other.vx -= fx;
        other.vy -= fy;
      }
    }

    for (const link of links) {
      const dx = link.target.x - link.source.x;
      const dy = link.target.y - link.source.y;
      const dist = Math.max(Math.hypot(dx, dy), 1);
      const force = dist * 0.0035 * Math.min(link.weight, 12);
      const fx = (dx / dist) * force;
      const fy = (dy / dist) * force;
      link.source.vx += fx;
      link.source.vy += fy;
      link.target.vx -= fx;
      link.target.vy -= fy;
    }

    for (const node of nodes) {
      node.vx += (centerX - node.x) * 0.0018;
      node.vy += (centerY - node.y) * 0.0018;
      node.vx *= 0.82;
      node.vy *= 0.82;
      node.x += node.vx;
      node.y += node.vy;
    }
  }
}
