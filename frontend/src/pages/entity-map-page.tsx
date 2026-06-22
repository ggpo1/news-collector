import { useState } from 'react';
import { EntityGraphView } from '../components/entity-graph/entity-graph';
import * as SectionS from '../components/section-page/section-page.styles';
import type { NamedEntityType } from '../api/entities';
import { useEntityGraph, type EntityGraphPeriod } from '../hooks/use-entity-graph/use-entity-graph';

export function EntityMapPage() {
  const [period, setPeriod] = useState<EntityGraphPeriod>('7d');
  const [entityType, setEntityType] = useState<NamedEntityType | ''>('');
  const [minWeight, setMinWeight] = useState(2);

  const { graph, loading, error, reload } = useEntityGraph({
    period,
    entityType,
    minWeight,
    maxNodes: 150,
  });

  return (
    <SectionS.SectionPage>
      <EntityGraphView
        graph={graph}
        loading={loading}
        error={error}
        period={period}
        entityType={entityType}
        minWeight={minWeight}
        onPeriodChange={setPeriod}
        onEntityTypeChange={setEntityType}
        onMinWeightChange={setMinWeight}
        onReload={() => void reload()}
      />
    </SectionS.SectionPage>
  );
}
