import { useEffect, useState } from 'react';
import { getEditorialTags } from '../../api/client';
import type { EditorialTag } from '../../api/types';

export function useEditorialTags() {
  const [tags, setTags] = useState<EditorialTag[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      try {
        setLoading(true);
        setError(null);
        const result = await getEditorialTags();
        if (!cancelled) {
          setTags(result);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Не удалось загрузить теги');
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    void load();

    return () => {
      cancelled = true;
    };
  }, []);

  return { tags, loading, error };
}
