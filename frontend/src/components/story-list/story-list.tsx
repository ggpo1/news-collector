import { STORY_STATUS_LABELS } from '../../api/story-labels';
import type { StoryListItem } from '../../api/types';
import { EmptyState } from '../ui/empty-state';
import { LoadingState } from '../ui/loading-state';
import * as S from './story-list.styles';

interface StoryListProps {
  items: StoryListItem[];
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
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(value));
}

export function StoryList({ items, loading, error, selectedId, onSelect }: StoryListProps) {
  if (loading) {
    return <LoadingState label="Загрузка тем…" />;
  }

  if (error) {
    return <EmptyState error>{error}</EmptyState>;
  }

  if (items.length === 0) {
    return (
      <EmptyState>
        Тем пока нет. Topic-linker создаёт связи, затем материализует темы автоматически.
      </EmptyState>
    );
  }

  return (
    <S.List>
      {items.map((item) => (
        <li key={item.id}>
          <S.Card type="button" $active={selectedId === item.id} onClick={() => onSelect(item.id)}>
            <S.Meta>
              <S.Badge>{STORY_STATUS_LABELS[item.status]}</S.Badge>
              <span>{item.sourceCount} ист.</span>
              <span>{item.articleCount} мат.</span>
              <time dateTime={item.lastActivityAt ?? undefined}>{formatDate(item.lastActivityAt)}</time>
            </S.Meta>
            <S.Title>{item.title}</S.Title>
            <S.Sources>{item.sourceNames.join(' · ')}</S.Sources>
          </S.Card>
        </li>
      ))}
    </S.List>
  );
}
