import { useCallback, useEffect, useState } from 'react';
import { getEditorialDashboard } from '../../api/client';
import type { EditorialDashboard } from '../../api/types';

export type DashboardWindowHours = 24 | 48 | 72;

export function useEditorialDashboard(windowHours: DashboardWindowHours = 48) {
  const [dashboard, setDashboard] = useState<EditorialDashboard | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await getEditorialDashboard(windowHours);
      setDashboard(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось загрузить обзор');
      setDashboard(null);
    } finally {
      setLoading(false);
    }
  }, [windowHours]);

  useEffect(() => {
    void load();
  }, [load]);

  return { dashboard, loading, error, reload: load };
}
