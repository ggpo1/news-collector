import { useState } from 'react';
import type { NewsRewrite } from '../../api/types';
import { SendToTelegramModal } from '../send-to-telegram-modal/send-to-telegram-modal';
import { SourceTaskCard } from '../source-task-card/source-task-card';
import * as S from './rewritten-news-detail.styles';

interface RewrittenNewsDetailProps {
  rewrite: NewsRewrite | null;
  onEdit: (rewrite: NewsRewrite) => void;
  onDelete: (rewrite: NewsRewrite) => Promise<void>;
  onOpenSourceNews?: (sourceNewsId: string, sourceId: string) => void;
}

function formatDate(value: string): string {
  return new Intl.DateTimeFormat('ru-RU', {
    dateStyle: 'long',
    timeStyle: 'short',
  }).format(new Date(value));
}

export function RewrittenNewsDetail({
  rewrite,
  onEdit,
  onDelete,
  onOpenSourceNews,
}: RewrittenNewsDetailProps) {
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [telegramOpen, setTelegramOpen] = useState(false);

  if (!rewrite) {
    return (
      <S.Panel>
        <S.Placeholder>Выберите переписанную новость из списка</S.Placeholder>
      </S.Panel>
    );
  }

  const handleDelete = async () => {
    if (!window.confirm('Удалить эту переписанную новость?')) {
      return;
    }

    setDeleting(true);
    setError(null);

    try {
      await onDelete(rewrite);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось удалить');
    } finally {
      setDeleting(false);
    }
  };

  return (
    <S.Panel>
      <S.Header>
        <S.Meta>
          <S.Badge>Перепись</S.Badge>
          <time dateTime={rewrite.createdAt}>создано {formatDate(rewrite.createdAt)}</time>
          <time dateTime={rewrite.updatedAt}>изменено {formatDate(rewrite.updatedAt)}</time>
        </S.Meta>
        <S.Title>{rewrite.title}</S.Title>
      </S.Header>

      <SourceTaskCard rewrite={rewrite} onOpenSourceNews={onOpenSourceNews} />

      {rewrite.summary && <S.Summary>{rewrite.summary}</S.Summary>}

      {rewrite.content ? (
        <S.Content>{rewrite.content}</S.Content>
      ) : (
        !rewrite.summary && <S.Summary>Текст не заполнен</S.Summary>
      )}

      {error && <S.Error>{error}</S.Error>}

      <S.Actions>
        <S.TelegramButton type="button" onClick={() => setTelegramOpen(true)}>
          Отправить в Telegram
        </S.TelegramButton>
        <S.PrimaryButton type="button" onClick={() => onEdit(rewrite)}>
          Редактировать
        </S.PrimaryButton>
        <S.DangerButton type="button" disabled={deleting} onClick={() => void handleDelete()}>
          {deleting ? 'Удаление…' : 'Удалить'}
        </S.DangerButton>
      </S.Actions>

      {telegramOpen && (
        <SendToTelegramModal
          title={rewrite.title}
          mode="rewrite"
          targetId={rewrite.id}
          sourceId={rewrite.sourceNewsSourceId}
          onClose={() => setTelegramOpen(false)}
        />
      )}
    </S.Panel>
  );
}
