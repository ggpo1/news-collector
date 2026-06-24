import { useCallback, useEffect, useState } from 'react';
import { getStories } from '../../api/client';
import type { StoryListItem, StoryStatus } from '../../api/types';

const PAGE_SIZE = 20;

export function useStories(status: StoryStatus | null) {
  const [items, setItems] = useState<StoryListItem[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await getStories({
        page,
        pageSize: PAGE_SIZE,
        status: status ?? undefined,
      });
      setItems(result.items);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось загрузить темы');
      setItems([]);
    } finally {
      setLoading(false);
    }
  }, [page, status]);

  useEffect(() => {
    setPage(1);
  }, [status]);

  useEffect(() => {
    void load();
  }, [load]);

  return { items, page, totalPages, totalCount, loading, error, setPage, reload: load };
}
