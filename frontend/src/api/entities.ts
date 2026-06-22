export type NamedEntityType =
  | 'Person'
  | 'Company'
  | 'Country'
  | 'Organization'
  | 'Location'
  | 'Event'
  | 'Other';

export interface EntityGraphNode {
  id: string;
  name: string;
  type: NamedEntityType;
  mentionCount: number;
}

export interface EntityGraphEdge {
  sourceId: string;
  targetId: string;
  weight: number;
}

export interface EntityGraph {
  from: string;
  to: string;
  nodes: EntityGraphNode[];
  edges: EntityGraphEdge[];
}

export interface NamedEntityNeighbor {
  entityId: string;
  name: string;
  type: NamedEntityType;
  coMentionCount: number;
}

export interface NamedEntityDetail {
  id: string;
  name: string;
  type: NamedEntityType;
  mentionCount: number;
  neighbors: NamedEntityNeighbor[];
}
