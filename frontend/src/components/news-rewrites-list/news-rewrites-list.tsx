import type { NewsRewrite } from '../../api/types';
import { SourceTaskCard } from '../source-task-card/source-task-card';
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
  return new Intl.DateTimeFormat('ru-RU', {
    dateStyle: 'medium',
    timeStyle: 'short',
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
    return <S.State>Загрузка переписанных новостей…</S.State>;
  }

  if (error) {
    return <S.State $error>{error}</S.State>;
  }

  if (items.length === 0) {
    return (
      <S.State>
        Переписанных новостей пока нет. Откройте новость и нажмите «Переписать».
      </S.State>
    );
  }

  return (
    <S.List>
      {items.map((item) => {
        const preview = previewText(item);

        return (
          <li key={item.id}>
            <S.Card type="button" $active={selectedId === item.id} onClick={() => onSelect(item.id)}>
              <S.Meta>
                <S.Badge>перепись</S.Badge>
                <time dateTime={item.updatedAt}>изменено {formatDate(item.updatedAt)}</time>
              </S.Meta>
              <S.Title>{item.title}</S.Title>
              {preview && <S.Preview>{preview}</S.Preview>}
            </S.Card>
            <S.TaskWrap>
              <SourceTaskCard rewrite={item} onOpenSourceNews={onOpenSourceNews} compact />
            </S.TaskWrap>
          </li>
        );
      })}
    </S.List>
  );
}
