import type { NewsRewrite } from '../../api/types';
import { SourceTaskCard } from '../source-task-card/source-task-card';
import { EmptyState } from '../ui/empty-state';
import { LoadingState } from '../ui/loading-state';
import * as S from './news-rewrites-list.styles';

interface NewsRewritesListProps {
  items: NewsRewrite[];
  loading: boolean;
  error: string | null;
  selectedId: string | null;
  onSelect: (id: string) => void;
  onOpenSourceNews?: (sourceNewsId: string, sourceId: string) => void;
}

function formatDate(value: string): string {
  return new Intl.DateTimeFormat("ru-RU", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

function previewText(rewrite: NewsRewrite): string | null {
  return rewrite.summary ?? rewrite.content;
}

export function NewsRewritesList({
  items,
  loading,
  error,
  selectedId,
  onSelect,
  onOpenSourceNews,
}: NewsRewritesListProps) {
  if (loading) {
    return <LoadingState label="Загрузка переписей…" />;
  }

  if (error) {
    return <EmptyState error>{error}</EmptyState>;
  }

  if (items.length === 0) {
    return (
      <EmptyState>
        Переписанных новостей пока нет. Откройте новость и нажмите «Переписать».
      </EmptyState>
    );
  }

  return (
    <S.List>
      {items.map((item) => {
        const preview = previewText(item);

        return (
          <li key={item.id}>
            <S.Card type="button" $active={selectedId === item.id} onClick={() => onSelect(item.id)}>
              <S.CardTop>
                <S.Meta>
                  <S.Badge>перепись</S.Badge>
                  <time dateTime={item.updatedAt}>изменено {formatDate(item.updatedAt)}</time>
                </S.Meta>
                <S.Chevron aria-hidden="true">›</S.Chevron>
              </S.CardTop>
              <S.Title>{item.title}</S.Title>
              {preview && <S.Preview>{preview}</S.Preview>}
              <S.TaskWrap>
                <SourceTaskCard rewrite={item} onOpenSourceNews={onOpenSourceNews} compact />
              </S.TaskWrap>
            </S.Card>
          </li>
        );
      })}
    </S.List>
  );
}
