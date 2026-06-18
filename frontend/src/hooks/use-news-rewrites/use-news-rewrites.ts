import { useCallback, useEffect, useState } from 'react';
import { getNewsRewrites } from '../../api/client';
import type { NewsRewrite } from '../../api/types';

const PAGE_SIZE = 20;

export function useNewsRewrites() {
  const [items, setItems] = useState<NewsRewrite[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await getNewsRewrites({ page, pageSize: PAGE_SIZE });
      setItems(result.items);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось загрузить переписи');
      setItems([]);
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    void load();
  }, [load]);

  return {
    items,
    page,
    totalPages,
    totalCount,
    loading,
    error,
    setPage,
    reload: load,
  };
}
