import type { NewsItemList } from '../../api/types';
import * as S from './news-list.styles';

interface NewsListProps {
  items: NewsItemList[];
  loading: boolean;
  error: string | null;
  selectedId: string | null;
  onSelect: (id: string) => void;
}

function formatDate(value: string | null): string {
  if (!value) {
    return '—';
  }

  return new Intl.DateTimeFormat('ru-RU', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}

export function NewsList({ items, loading, error, selectedId, onSelect }: NewsListProps) {
  if (loading) {
    return <S.State>Загрузка новостей…</S.State>;
  }

  if (error) {
    return <S.State $error>{error}</S.State>;
  }

  if (items.length === 0) {
    return <S.State>Новостей пока нет</S.State>;
  }

  return (
    <S.List>
      {items.map((item) => (
        <li key={item.id}>
          <S.Card type="button" $active={selectedId === item.id} onClick={() => onSelect(item.id)}>
            <S.Meta>
              <time dateTime={item.publishedAt ?? undefined}>{formatDate(item.publishedAt)}</time>
              {item.hasContent && <S.Badge>полный текст</S.Badge>}
            </S.Meta>
            <S.Title>{item.title}</S.Title>
            {item.summary && <S.Summary>{item.summary}</S.Summary>}
          </S.Card>
        </li>
      ))}
    </S.List>
  );
}
