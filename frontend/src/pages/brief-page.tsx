import { useState } from 'react';
import { EditorialBriefView } from '../components/editorial-brief/editorial-brief';
import * as SectionS from '../components/section-page/section-page.styles';
import { useAuth } from '../contexts/auth-context';
import {
  useEditorialBrief,
  type BriefPeriodFilter,
} from '../hooks/use-editorial-brief/use-editorial-brief';

export function BriefPage() {
  const { isChiefEditor } = useAuth();
  const [periodFilter, setPeriodFilter] = useState<BriefPeriodFilter>('latest');
  const { brief, loading, generating, error, reload, generate } = useEditorialBrief(periodFilter);

  const handleGenerate = () => {
    const period = periodFilter === 'Evening' ? 'Evening' : 'Morning';
    void generate(period);
  };

  return (
    <SectionS.SectionPage>
      <EditorialBriefView
        brief={brief}
        loading={loading}
        generating={generating}
        error={error}
        periodFilter={periodFilter}
        onPeriodFilterChange={setPeriodFilter}
        onReload={() => void reload()}
        onGenerate={handleGenerate}
        canGenerate={isChiefEditor}
      />
    </SectionS.SectionPage>
  );
}
