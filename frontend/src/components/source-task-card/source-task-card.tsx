import type { NewsRewrite } from '../../api/types';
import * as S from './source-task-card.styles';

interface SourceTaskCardProps {
  rewrite: NewsRewrite;
  onOpenSourceNews?: (sourceNewsId: string, sourceId: string) => void;
  compact?: boolean;
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

export function SourceTaskCard({ rewrite, onOpenSourceNews, compact = false }: SourceTaskCardProps) {
  return (
    <S.Card $compact={compact}>
      <S.Label>Исходная новость</S.Label>
      <S.Meta>
        <S.Badge>{rewrite.sourceNewsSourceName}</S.Badge>
        {rewrite.sourceNewsPublishedAt && (
          <time dateTime={rewrite.sourceNewsPublishedAt}>
            {formatDate(rewrite.sourceNewsPublishedAt)}
          </time>
        )}
      </S.Meta>
      <S.Title>{rewrite.sourceNewsTitle}</S.Title>
      <S.Actions>
        {onOpenSourceNews && (
          <S.LinkButton
            type="button"
            onClick={() => onOpenSourceNews(rewrite.sourceNewsId, rewrite.sourceNewsSourceId)}
          >
            Открыть в новостях
          </S.LinkButton>
        )}
        <S.ExternalLink href={rewrite.sourceNewsUrl} target="_blank" rel="noreferrer">
          Оригинал на сайте →
        </S.ExternalLink>
      </S.Actions>
    </S.Card>
  );
}
