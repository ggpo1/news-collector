import { useState } from 'react';
import type { NewsRewrite } from '../../api/types';
import * as S from './rewritten-news-detail.styles';

interface RewrittenNewsDetailProps {
  rewrite: NewsRewrite | null;
  onEdit: (rewrite: NewsRewrite) => void;
  onDelete: (rewrite: NewsRewrite) => Promise<void>;
}

function formatDate(value: string): string {
  return new Intl.DateTimeFormat('ru-RU', {
    dateStyle: 'long',
    timeStyle: 'short',
  }).format(new Date(value));
}

export function RewrittenNewsDetail({ rewrite, onEdit, onDelete }: RewrittenNewsDetailProps) {
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);

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

      <S.SourceBlock>
        <S.SourceLabel>Исходная новость</S.SourceLabel>
        <S.SourceTitle>{rewrite.sourceNewsTitle}</S.SourceTitle>
      </S.SourceBlock>

      {rewrite.summary && <S.Summary>{rewrite.summary}</S.Summary>}

      {rewrite.content ? (
        <S.Content>{rewrite.content}</S.Content>
      ) : (
        !rewrite.summary && <S.Summary>Текст не заполнен</S.Summary>
      )}

      {error && <S.Error>{error}</S.Error>}

      <S.Actions>
        <S.PrimaryButton type="button" onClick={() => onEdit(rewrite)}>
          Редактировать
        </S.PrimaryButton>
        <S.DangerButton type="button" disabled={deleting} onClick={() => void handleDelete()}>
          {deleting ? 'Удаление…' : 'Удалить'}
        </S.DangerButton>
      </S.Actions>
    </S.Panel>
  );
}
