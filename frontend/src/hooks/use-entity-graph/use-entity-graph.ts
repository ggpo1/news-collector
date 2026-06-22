import { useCallback, useEffect, useState } from 'react';
import { getEntityGraph } from '../../api/client';
import type { EntityGraph, NamedEntityType } from '../../api/entities';

export type EntityGraphPeriod = '1d' | '7d' | '30d';

function periodToRange(period: EntityGraphPeriod): { from: string; to: string } {
  const to = new Date();
  const from = new Date();

  switch (period) {
    case '1d':
      from.setDate(from.getDate() - 1);
      break;
    case '30d':
      from.setDate(from.getDate() - 30);
      break;
    case '7d':
    default:
      from.setDate(from.getDate() - 7);
      break;
  }

  return { from: from.toISOString(), to: to.toISOString() };
}

interface UseEntityGraphOptions {
  period: EntityGraphPeriod;
  entityType: NamedEntityType | '';
  minWeight: number;
  maxNodes: number;
}

export function useEntityGraph({
  period,
  entityType,
  minWeight,
  maxNodes,
}: UseEntityGraphOptions) {
  const [graph, setGraph] = useState<EntityGraph | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const reload = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const range = periodToRange(period);
      const data = await getEntityGraph({
        ...range,
        type: entityType || undefined,
        minWeight,
        maxNodes,
      });
      setGraph(data);
    } catch (err) {
      setGraph(null);
      setError(err instanceof Error ? err.message : 'Не удалось загрузить граф');
    } finally {
      setLoading(false);
    }
  }, [entityType, maxNodes, minWeight, period]);

  useEffect(() => {
    void reload();
  }, [reload]);

  return { graph, loading, error, reload };
}
