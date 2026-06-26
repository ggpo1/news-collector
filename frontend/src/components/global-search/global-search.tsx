import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { searchDocuments } from '../../api/client';
import type { SearchDocumentType, SearchResult } from '../../api/types';
import { buildNewsPath } from '../../app/news-route';
import { PATHS } from '../../app/paths';
import * as S from './global-search.styles';

const TYPE_LABELS: Record<SearchDocumentType, string> = {
  News: 'Новость',
  Story: 'Тема',
  Rewrite: 'Перепись',
};

export function GlobalSearch() {
  const navigate = useNavigate();
  const rootRef = useRef<HTMLDivElement>(null);
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<SearchResult[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [open, setOpen] = useState(false);

  useEffect(() => {
    const trimmed = query.trim();
    if (trimmed.length < 2) {
      setResults([]);
      setError(null);
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);

    const timeoutId = window.setTimeout(() => {
      void searchDocuments(trimmed)
        .then((items) => {
          setResults(items);
          setOpen(true);
        })
        .catch((err) => {
          setResults([]);
          setError(err instanceof Error ? err.message : 'Ошибка поиска');
          setOpen(true);
        })
        .finally(() => setLoading(false));
    }, 300);

    return () => window.clearTimeout(timeoutId);
  }, [query]);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (!rootRef.current?.contains(event.target as Node)) {
        setOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleSelect = (result: SearchResult) => {
    setOpen(false);
    setQuery('');

    if (result.documentType === 'News') {
      navigate(
        buildNewsPath({
          page: 1,
          sourceId: null,
          categoryId: null,
          uncategorized: false,
          tone: null,
          newsId: result.entityId,
        }),
      );
      return;
    }

    if (result.documentType === 'Story') {
      navigate(PATHS.stories, { state: { storyId: result.entityId } });
      return;
    }

    navigate(PATHS.rewrites);
  };

  const showDropdown = open && query.trim().length >= 2;

  return (
    <S.Root ref={rootRef}>
      <S.Input
        type="search"
        value={query}
        placeholder="Поиск по новостям…"
        aria-label="Глобальный поиск"
        onChange={(event) => {
          setQuery(event.target.value);
          setOpen(true);
        }}
        onFocus={() => setOpen(true)}
      />

      {showDropdown && (
        <S.Dropdown role="listbox">
          {loading && <S.Status>Ищем…</S.Status>}
          {!loading && error && <S.Status>{error}</S.Status>}
          {!loading && !error && results.length === 0 && (
            <S.Status>Ничего не найдено</S.Status>
          )}
          {!loading &&
            !error &&
            results.map((result) => (
              <S.ResultButton
                key={`${result.documentType}-${result.entityId}`}
                type="button"
                onClick={() => handleSelect(result)}
              >
                <S.ResultTitle>{result.title}</S.ResultTitle>
                <S.ResultMeta>
                  {TYPE_LABELS[result.documentType]}
                  {result.sourceName ? ` · ${result.sourceName}` : ''}
                </S.ResultMeta>
                {result.snippet && (
                  <S.ResultSnippet dangerouslySetInnerHTML={{ __html: result.snippet }} />
                )}
              </S.ResultButton>
            ))}
        </S.Dropdown>
      )}
    </S.Root>
  );
}
