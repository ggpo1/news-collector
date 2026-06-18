import { useCallback, useEffect, useState } from 'react';
import { enrichNewsContent, getNewsById } from '../../api/client';
import type { NewsItemDetail } from '../../api/types';
import * as S from './news-detail.styles';

interface NewsDetailProps {
  newsId: string | null;
  onContentLoaded?: (newsId: string) => void;
}

function formatDate(value: string | null): string {
  if (!value) {
    return '—';
  }

  return new Intl.DateTimeFormat('ru-RU', {
    dateStyle: 'long',
    timeStyle: 'short',
  }).format(new Date(value));
}

export function NewsDetail({ newsId, onContentLoaded }: NewsDetailProps) {
  const [item, setItem] = useState<NewsItemDetail | null>(null);
  const [loading, setLoading] = useState(false);
  const [enriching, setEnriching] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [enrichError, setEnrichError] = useState<string | null>(null);

  const loadItem = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);

    try {
      const data = await getNewsById(id);
      setItem(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось загрузить новость');
      setItem(null);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!newsId) {
      setItem(null);
      setError(null);
      setEnrichError(null);
      return;
    }

    void loadItem(newsId);
  }, [newsId, loadItem]);

  const handleEnrichContent = async () => {
    if (!newsId || !item) {
      return;
    }

    setEnriching(true);
    setEnrichError(null);

    try {
      const result = await enrichNewsContent(newsId);

      if (result.item) {
        setItem(result.item);
      }

      if (result.status === 'enriched' || result.status === 'already_enriched') {
        onContentLoaded?.(newsId);
        return;
      }

      setEnrichError(result.message ?? 'Не удалось загрузить полный текст');
    } catch (err) {
      setEnrichError(err instanceof Error ? err.message : 'Не удалось загрузить полный текст');
    } finally {
      setEnriching(false);
    }
  };

  if (!newsId) {
    return (
      <S.Panel>
        <S.Placeholder>Выберите новость из списка</S.Placeholder>
      </S.Panel>
    );
  }

  if (loading) {
    return (
      <S.Panel>
        <S.State>Загрузка…</S.State>
      </S.Panel>
    );
  }

  if (error) {
    return (
      <S.Panel>
        <S.State $error>{error}</S.State>
      </S.Panel>
    );
  }

  if (!item) {
    return null;
  }

  const showFetchButton = !item.content;

  return (
    <S.Panel>
      <S.Header>
        <S.Meta>
          <span>{item.sourceName}</span>
          <time dateTime={item.publishedAt ?? undefined}>{formatDate(item.publishedAt)}</time>
        </S.Meta>
        <S.Title>{item.title}</S.Title>
      </S.Header>

      {item.content ? (
        <S.Content>{item.content}</S.Content>
      ) : item.summary ? (
        <S.Content>{item.summary}</S.Content>
      ) : (
        <S.State>Полный текст ещё не загружен</S.State>
      )}

      {enrichError && <S.FetchError>{enrichError}</S.FetchError>}

      <S.Actions>
        {showFetchButton && (
          <S.FetchButton type="button" disabled={enriching} onClick={() => void handleEnrichContent()}>
            {enriching ? 'Загрузка текста…' : 'Загрузить полный текст'}
          </S.FetchButton>
        )}
        <S.ExternalLink href={item.url} target="_blank" rel="noreferrer">
          Открыть оригинал →
        </S.ExternalLink>
      </S.Actions>
    </S.Panel>
  );
}
