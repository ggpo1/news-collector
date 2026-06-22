import { useCallback, useEffect, useState } from 'react';
import { enrichNewsContent, getNewsById, getRelatedNews } from '../../api/client';
import { formatConfidence, LINK_METHOD_LABELS, LINK_TYPE_LABELS } from '../../api/link-labels';
import type { NewsItemDetail, RelatedNews } from '../../api/types';
import { NewsRewriteEditor } from '../news-rewrite-editor/news-rewrite-editor';
import { EmptyState } from '../ui/empty-state';
import { LoadingState } from '../ui/loading-state';
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
  const [related, setRelated] = useState<RelatedNews[]>([]);
  const [relatedLoading, setRelatedLoading] = useState(false);
  const [editorOpen, setEditorOpen] = useState(false);

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
      setRelated([]);
      setEditorOpen(false);
      return;
    }

    void loadItem(newsId);
  }, [newsId, loadItem]);

  useEffect(() => {
    if (!newsId) {
      return;
    }

    let cancelled = false;

    const loadRelated = async () => {
      setRelatedLoading(true);

      try {
        const data = await getRelatedNews(newsId);
        if (!cancelled) {
          setRelated(data);
        }
      } catch {
        if (!cancelled) {
          setRelated([]);
        }
      } finally {
        if (!cancelled) {
          setRelatedLoading(false);
        }
      }
    };

    void loadRelated();

    return () => {
      cancelled = true;
    };
  }, [newsId]);

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
        <LoadingState />
      </S.Panel>
    );
  }

  if (error) {
    return (
      <S.Panel>
        <EmptyState error>{error}</EmptyState>
      </S.Panel>
    );
  }

  if (!item) {
    return null;
  }

  const showFetchButton = !item.content;

  return (
    <>
      <S.Panel>
      <S.Header>
        <S.Meta>
          <span>{item.sourceName}</span>
          {item.categoryName ? (
            <S.CategoryBadge>{item.categoryName}</S.CategoryBadge>
          ) : (
            <S.CategoryBadge $muted>без категории</S.CategoryBadge>
          )}
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
        <S.RewriteButton type="button" onClick={() => setEditorOpen(true)}>
          Переписать
        </S.RewriteButton>
        <S.ExternalLink href={item.url} target="_blank" rel="noreferrer">
          Открыть оригинал →
        </S.ExternalLink>
      </S.Actions>

      <S.RelatedSection>
        <S.RelatedTitle>Связанные новости</S.RelatedTitle>
        {relatedLoading ? (
          <S.RelatedState>Загрузка связей…</S.RelatedState>
        ) : related.length === 0 ? (
          <S.RelatedState>Связей пока нет</S.RelatedState>
        ) : (
          <S.RelatedList>
            {related.map((entry) => (
              <S.RelatedItem key={entry.linkId}>
                <S.RelatedMeta>
                  <S.RelatedBadge>{LINK_TYPE_LABELS[entry.linkType]}</S.RelatedBadge>
                  <span>{formatConfidence(entry.confidence)}</span>
                  <span>{LINK_METHOD_LABELS[entry.linkMethod]}</span>
                </S.RelatedMeta>
                <S.RelatedNewsTitle>{entry.news.title}</S.RelatedNewsTitle>
                <S.RelatedSource>{entry.news.sourceName}</S.RelatedSource>
              </S.RelatedItem>
            ))}
          </S.RelatedList>
        )}
      </S.RelatedSection>
      </S.Panel>

      {editorOpen && (
        <NewsRewriteEditor
          sourceNews={item}
          onClose={() => setEditorOpen(false)}
        />
      )}
    </>
  );
}
