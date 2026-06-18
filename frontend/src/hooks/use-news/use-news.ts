import { useCallback, useEffect, useState } from 'react';
import { getNews } from '../../api/client';
import type { NewsItemList } from '../../api/types';

const PAGE_SIZE = 20;

export function useNews(sourceId: string | null) {
  const [items, setItems] = useState<NewsItemList[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!sourceId) {
      setItems([]);
      return;
    }

    try {
      setLoading(true);
      setError(null);
      const result = await getNews({ page, pageSize: PAGE_SIZE, sourceId });
      setItems(result.items);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load news');
      setItems([]);
    } finally {
      setLoading(false);
    }
  }, [page, sourceId]);

  useEffect(() => {
    setPage(1);
  }, [sourceId]);

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
