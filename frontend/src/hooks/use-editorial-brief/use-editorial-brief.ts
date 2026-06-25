import { useCallback, useEffect, useState } from 'react';
import { generateEditorialBrief, getEditorialBrief } from '../../api/client';
import type { EditorialBriefReport } from '../../api/types';

export type BriefPeriodFilter = 'latest' | 'Morning' | 'Evening';

export function useEditorialBrief(periodFilter: BriefPeriodFilter = 'latest') {
  const [brief, setBrief] = useState<EditorialBriefReport | null>(null);
  const [loading, setLoading] = useState(false);
  const [generating, setGenerating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const period = periodFilter === 'latest' ? undefined : periodFilter;
      const result = await getEditorialBrief(period);
      setBrief(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось загрузить бриф');
      setBrief(null);
    } finally {
      setLoading(false);
    }
  }, [periodFilter]);

  const generate = useCallback(
    async (period: 'Morning' | 'Evening' = 'Morning') => {
      try {
        setGenerating(true);
        setError(null);
        const result = await generateEditorialBrief(period);
        setBrief(result);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Не удалось сгенерировать бриф');
      } finally {
        setGenerating(false);
      }
    },
    [],
  );

  useEffect(() => {
    void load();
  }, [load]);

  return { brief, loading, generating, error, reload: load, generate };
}
