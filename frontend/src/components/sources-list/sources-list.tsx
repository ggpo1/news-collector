import type { Source, SourceType } from '../../api/types';
import * as S from './sources-list.styles';

interface SourcesListProps {
  items: Source[];
  deletingId: string | null;
  onEdit: (source: Source) => void;
  onDelete: (source: Source) => void;
}

function formatDate(value: string | null): string {
  if (!value) {
    return 'ещё не опрашивался';
  }

  return new Intl.DateTimeFormat('ru-RU', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}

function formatSourceType(type: SourceType | number): string {
  if (typeof type === 'string') {
    return type;
  }

  return ['Rss', 'Html', 'Api'][type] ?? String(type);
}

export function SourcesList({ items, deletingId, onEdit, onDelete }: SourcesListProps) {
  return (
    <S.List>
      {items.map((source) => (
        <li key={source.id}>
          <S.Card>
            <S.CardHeader>
              <div>
                <S.Title>{source.name}</S.Title>
                <S.Meta>
                  <span>{formatSourceType(source.type)}</span>
                  <span>·</span>
                  <span>каждые {source.fetchIntervalMinutes} мин</span>
                  <span>·</span>
                  <span>последний опрос: {formatDate(source.lastFetchedAt)}</span>
                </S.Meta>
              </div>
              <S.Badge $muted={!source.isActive}>{source.isActive ? 'активен' : 'выключен'}</S.Badge>
            </S.CardHeader>

            <S.Url href={source.url} target="_blank" rel="noreferrer">
              {source.url}
            </S.Url>

            <S.Actions>
              <S.ActionButton type="button" onClick={() => onEdit(source)}>
                Редактировать
              </S.ActionButton>
              <S.DangerButton
                type="button"
                disabled={deletingId === source.id}
                onClick={() => onDelete(source)}
              >
                {deletingId === source.id ? 'Удаление…' : 'Удалить'}
              </S.DangerButton>
            </S.Actions>
          </S.Card>
        </li>
      ))}
    </S.List>
  );
}
