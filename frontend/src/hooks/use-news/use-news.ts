import { useCallback, useEffect, useState } from 'react';
import { getNews } from '../../api/client';
import type { NewsItemList } from '../../api/types';
import type { NewsToneFilterValue } from '../../components/tone-select/tone-select';

const PAGE_SIZE = 20;

export interface NewsFilters {
  sourceId: string | null;
  categoryId: string | null;
  uncategorized: boolean;
  toneFilter: NewsToneFilterValue | null;
  editorialTagId: string | null;
}

export function useNews({
  page,
  sourceId,
  categoryId,
  uncategorized,
  toneFilter,
  editorialTagId,
}: NewsFilters & { page: number }) {
  const [items, setItems] = useState<NewsItemList[]>([]);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await getNews({
        page,
        pageSize: PAGE_SIZE,
        sourceId: sourceId ?? undefined,
        categoryId: uncategorized ? undefined : (categoryId ?? undefined),
        uncategorized: uncategorized || undefined,
        toneFilter: toneFilter ?? undefined,
        editorialTagId: editorialTagId ?? undefined,
      });
      setItems(result.items);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load news');
      setItems([]);
    } finally {
      setLoading(false);
    }
  }, [page, sourceId, categoryId, uncategorized, toneFilter, editorialTagId]);

  useEffect(() => {
    void load();
  }, [load]);

  return {
    items,
    totalPages,
    totalCount,
    loading,
    error,
    reload: load,
  };
}
