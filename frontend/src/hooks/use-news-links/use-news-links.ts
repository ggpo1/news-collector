import { useCallback, useEffect, useState } from 'react';
import { getNewsLinks } from '../../api/client';
import type { LinkType, NewsLink } from '../../api/types';

const PAGE_SIZE = 20;

export function useNewsLinks(linkType: LinkType | null) {
  const [items, setItems] = useState<NewsLink[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await getNewsLinks({
        page,
        pageSize: PAGE_SIZE,
        linkType: linkType ?? undefined,
      });
      setItems(result.items);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось загрузить связи');
      setItems([]);
    } finally {
      setLoading(false);
    }
  }, [linkType, page]);

  useEffect(() => {
    setPage(1);
  }, [linkType]);

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
