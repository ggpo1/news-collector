import { formatConfidence, LINK_METHOD_LABELS, LINK_TYPE_LABELS } from '../../api/link-labels';
import type { LinkType, NewsLink } from '../../api/types';
import { EmptyState } from '../ui/empty-state';
import { LoadingState } from '../ui/loading-state';
import * as S from './news-links-list.styles';

interface NewsLinksListProps {
  items: NewsLink[];
  loading: boolean;
  error: string | null;
  selectedId: string | null;
  onSelect: (id: string) => void;
}

function formatDate(value: string): string {
  return new Intl.DateTimeFormat('ru-RU', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}

function badgeVariant(linkType: LinkType): 'topic' | 'duplicate' | 'related' | undefined {
  if (linkType === 'Duplicate') {
    return 'duplicate';
  }

  if (linkType === 'Related') {
    return 'related';
  }

  return 'topic';
}

export function NewsLinksList({ items, loading, error, selectedId, onSelect }: NewsLinksListProps) {
  if (loading) {
    return <LoadingState label="Загрузка связей…" />;
  }

  if (error) {
    return <EmptyState error>{error}</EmptyState>;
  }

  if (items.length === 0) {
    return <EmptyState>Связей пока нет. Topic Linker создаёт их автоматически.</EmptyState>;
  }

  return (
    <S.List>
      {items.map((item) => (
        <li key={item.id}>
          <S.Card type="button" $active={selectedId === item.id} onClick={() => onSelect(item.id)}>
            <S.CardTop>
              <S.Meta>
              <S.Badge $variant={badgeVariant(item.linkType)}>{LINK_TYPE_LABELS[item.linkType]}</S.Badge>
              <span>{formatConfidence(item.confidence)}</span>
              <span>{LINK_METHOD_LABELS[item.linkMethod]}</span>
              <time dateTime={item.createdAt}>{formatDate(item.createdAt)}</time>
              </S.Meta>
              <S.Chevron aria-hidden="true">›</S.Chevron>
            </S.CardTop>
            <S.Pair>
              <S.NewsLine>
                <S.Source>{item.newsLow.sourceName}</S.Source>
                <S.Title>{item.newsLow.title}</S.Title>
              </S.NewsLine>
              <S.Connector>↕</S.Connector>
              <S.NewsLine>
                <S.Source>{item.newsHigh.sourceName}</S.Source>
                <S.Title>{item.newsHigh.title}</S.Title>
              </S.NewsLine>
            </S.Pair>
          </S.Card>
        </li>
      ))}
    </S.List>
  );
}
