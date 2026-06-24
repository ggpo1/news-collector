import type { NewsItemList } from '../../api/types';
import { formatToneCoefficient } from '../../api/tone';
import { EmptyState } from '../ui/empty-state';
import { LoadingState } from '../ui/loading-state';
import * as S from './news-list.styles';

interface NewsListProps {
  items: NewsItemList[];
  loading: boolean;
  error: string | null;
  selectedId: string | null;
  showSource?: boolean;
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

export function NewsList({
  items,
  loading,
  error,
  selectedId,
  showSource = true,
  onSelect,
}: NewsListProps) {
  if (loading) {
    return <LoadingState label="Загрузка новостей…" />;
  }

  if (error) {
    return <EmptyState error>{error}</EmptyState>;
  }

  if (items.length === 0) {
    return <EmptyState>Новостей пока нет. Worker подтянет их при следующем опросе источника.</EmptyState>;
  }

  return (
    <S.List>
      {items.map((item) => (
        <li key={item.id}>
          <S.Card
            type="button"
            $active={selectedId === item.id}
            onClick={() => onSelect(item.id)}
          >
            <S.CardTop>
              <S.Meta>
                {showSource && <S.SourceName>{item.sourceName}</S.SourceName>}
                <time dateTime={item.publishedAt ?? undefined}>{formatDate(item.publishedAt)}</time>
                {item.categoryName ? (
                  <S.CategoryBadge>{item.categoryName}</S.CategoryBadge>
                ) : (
                  <S.CategoryBadge $muted>без категории</S.CategoryBadge>
                )}
                {formatToneCoefficient(item.toneCoefficient) ? (
                  <S.ToneBadge $value={item.toneCoefficient}>
                    тон {formatToneCoefficient(item.toneCoefficient)}
                  </S.ToneBadge>
                ) : (
                  <S.ToneBadge $muted>тон —</S.ToneBadge>
                )}
                {item.hasContent && <S.Badge>полный текст</S.Badge>}
              </S.Meta>
              <S.Chevron aria-hidden="true">›</S.Chevron>
            </S.CardTop>
            <S.Title>{item.title}</S.Title>
            {item.summary && <S.Summary>{item.summary}</S.Summary>}
          </S.Card>
        </li>
      ))}
    </S.List>
  );
}
