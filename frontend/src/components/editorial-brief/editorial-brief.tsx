import { type ReactNode } from 'react';
import type { EditorialBriefReport } from '../../api/types';
import { LoadingState } from '../ui/loading-state';
import * as S from './editorial-brief.styles';

interface EditorialBriefViewProps {
  brief: EditorialBriefReport | null;
  loading: boolean;
  generating: boolean;
  error: string | null;
  periodFilter: 'latest' | 'Morning' | 'Evening';
  onPeriodFilterChange: (value: 'latest' | 'Morning' | 'Evening') => void;
  onReload: () => void;
  onGenerate: () => void;
  canGenerate: boolean;
}

function renderInlineMarkdown(text: string) {
  const parts = text.split(/(\*\*[^*]+\*\*)/g);
  return parts.map((part, index) => {
    if (part.startsWith('**') && part.endsWith('**')) {
      return <strong key={index}>{part.slice(2, -2)}</strong>;
    }

    return part;
  });
}

function MarkdownContent({ markdown }: { markdown: string }) {
  const blocks = markdown.split('\n');
  const elements: ReactNode[] = [];
  let listItems: ReactNode[] = [];

  const flushList = () => {
    if (listItems.length > 0) {
      elements.push(<ul key={`list-${elements.length}`}>{listItems}</ul>);
      listItems = [];
    }
  };

  blocks.forEach((line, index) => {
    const trimmed = line.trim();

    if (trimmed.startsWith('## ')) {
      flushList();
      elements.push(<h2 key={index}>{renderInlineMarkdown(trimmed.slice(3))}</h2>);
      return;
    }

    if (trimmed.startsWith('### ')) {
      flushList();
      elements.push(<h3 key={index}>{renderInlineMarkdown(trimmed.slice(4))}</h3>);
      return;
    }

    if (trimmed.startsWith('- ')) {
      listItems.push(<li key={index}>{renderInlineMarkdown(trimmed.slice(2))}</li>);
      return;
    }

    flushList();

    if (trimmed.length > 0) {
      elements.push(<p key={index}>{renderInlineMarkdown(trimmed)}</p>);
    }
  });

  flushList();
  return <S.MarkdownBody>{elements}</S.MarkdownBody>;
}

function formatWhen(value: string) {
  return new Date(value).toLocaleString('ru-RU', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function periodLabel(period: string) {
  return period === 'Morning' ? 'Утренний' : period === 'Evening' ? 'Вечерний' : period;
}

export function EditorialBriefView({
  brief,
  loading,
  generating,
  error,
  periodFilter,
  onPeriodFilterChange,
  onReload,
  onGenerate,
  canGenerate,
}: EditorialBriefViewProps) {
  return (
    <S.Page>
      <S.Toolbar>
        <S.ToolbarGroup>
          <S.Select
            value={periodFilter}
            disabled={loading || generating}
            onChange={(event) =>
              onPeriodFilterChange(event.target.value as 'latest' | 'Morning' | 'Evening')
            }
            aria-label="Период брифа"
          >
            <option value="latest">Последний бриф</option>
            <option value="Morning">Утренний</option>
            <option value="Evening">Вечерний</option>
          </S.Select>
          <S.Button type="button" disabled={loading || generating} onClick={onReload}>
            Обновить
          </S.Button>
          {canGenerate && (
            <S.PrimaryButton type="button" disabled={loading || generating} onClick={onGenerate}>
              {generating ? 'Генерируем…' : 'Сгенерировать сейчас'}
            </S.PrimaryButton>
          )}
        </S.ToolbarGroup>
        {brief && (
          <S.MetaLine>
            {periodLabel(brief.period)} · окно {formatWhen(brief.windowStart)} — {formatWhen(brief.windowEnd)} ·{' '}
            обновлено {formatWhen(brief.generatedAt)}
            {brief.model ? ` · ${brief.model}` : ''}
          </S.MetaLine>
        )}
      </S.Toolbar>

      {error && <S.ErrorText>{error}</S.ErrorText>}
      {loading && !brief ? <LoadingState label="Загружаем бриф…" /> : null}

      {!loading && !brief && !error && (
        <S.EmptyState>
          Бриф ещё не сгенерирован. Воркер <code>editorial-brief</code> создаёт его по расписанию (UTC 6:00 и
          18:00).
          {canGenerate ? ' Или нажмите «Сгенерировать сейчас».' : ''}
        </S.EmptyState>
      )}

      {brief && <MarkdownContent markdown={brief.markdown} />}
    </S.Page>
  );
}
