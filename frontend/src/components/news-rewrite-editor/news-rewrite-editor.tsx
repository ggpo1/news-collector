import { useCallback, useEffect, useState } from 'react';
import { createNewsRewrite, getNewsRewrites, updateNewsRewrite } from '../../api/client';
import type { NewsItemDetail, NewsRewrite } from '../../api/types';
import * as S from './news-rewrite-editor.styles';

interface NewsRewriteEditorProps {
  sourceNews: NewsItemDetail;
  initialRewrite?: NewsRewrite | null;
  onClose: () => void;
  onSaved?: () => void;
}

interface EditorForm {
  title: string;
  summary: string;
  content: string;
}

function buildInitialForm(sourceNews: NewsItemDetail): EditorForm {
  return {
    title: sourceNews.title,
    summary: sourceNews.summary ?? '',
    content: sourceNews.content ?? sourceNews.summary ?? '',
  };
}

function buildFormFromRewrite(rewrite: NewsRewrite): EditorForm {
  return {
    title: rewrite.title,
    summary: rewrite.summary ?? '',
    content: rewrite.content ?? '',
  };
}

function toPayload(form: EditorForm) {
  return {
    title: form.title.trim(),
    summary: form.summary.trim() || null,
    content: form.content.trim() || null,
  };
}

export function NewsRewriteEditor({
  sourceNews,
  initialRewrite = null,
  onClose,
  onSaved,
}: NewsRewriteEditorProps) {
  const [rewriteId, setRewriteId] = useState<string | null>(null);
  const [form, setForm] = useState<EditorForm>(() => buildInitialForm(sourceNews));
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const loadExistingRewrite = useCallback(async () => {
    setLoading(true);
    setError(null);
    setSuccessMessage(null);

    if (initialRewrite) {
      setRewriteId(initialRewrite.id);
      setForm(buildFormFromRewrite(initialRewrite));
      setLoading(false);
      return;
    }

    try {
      const result = await getNewsRewrites({
        page: 1,
        pageSize: 1,
        sourceNewsId: sourceNews.id,
      });

      const existing = result.items[0];
      if (existing) {
        setRewriteId(existing.id);
        setForm({
          title: existing.title,
          summary: existing.summary ?? '',
          content: existing.content ?? '',
        });
      } else {
        setRewriteId(null);
        setForm(buildInitialForm(sourceNews));
      }
    } catch (err) {
      setRewriteId(null);
      setForm(buildInitialForm(sourceNews));
      setError(err instanceof Error ? err.message : 'Не удалось загрузить черновик');
    } finally {
      setLoading(false);
    }
  }, [initialRewrite, sourceNews]);

  useEffect(() => {
    void loadExistingRewrite();
  }, [loadExistingRewrite]);

  const handleFieldChange = (field: keyof EditorForm, value: string) => {
    setForm((current) => ({ ...current, [field]: value }));
    setSuccessMessage(null);
  };

  const handleSave = async () => {
    const payload = toPayload(form);

    if (!payload.title) {
      setError('Заголовок не может быть пустым');
      return;
    }

    setSaving(true);
    setError(null);
    setSuccessMessage(null);

    try {
      const saved = rewriteId
        ? await updateNewsRewrite(rewriteId, payload)
        : await createNewsRewrite({
            sourceNewsId: sourceNews.id,
            ...payload,
          });

      setRewriteId(saved.id);
      setSuccessMessage('Переписанная новость сохранена');
      onSaved?.();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось сохранить');
    } finally {
      setSaving(false);
    }
  };

  const handleAiRewrite = () => {
    // Заглушка: интеграция с ИИ будет добавлена позже.
  };

  return (
    <S.Overlay role="presentation">
      <S.Dialog role="dialog" aria-modal="true" aria-labelledby="rewrite-editor-title">
        <S.Header>
          <S.HeaderText>
            <S.Title id="rewrite-editor-title">Редактор переписывания</S.Title>
            <S.Subtitle>
              Исходная новость: {sourceNews.title}
              {rewriteId ? ' · редактируется сохранённая версия' : ' · новый черновик'}
            </S.Subtitle>
          </S.HeaderText>
          <S.CloseButton type="button" aria-label="Закрыть" onClick={onClose}>
            ×
          </S.CloseButton>
        </S.Header>

        {loading ? (
          <S.Body>
            <S.State>Загрузка редактора…</S.State>
          </S.Body>
        ) : (
          <S.Body>
            <S.Field>
              Заголовок
              <S.Input
                value={form.title}
                onChange={(event) => handleFieldChange('title', event.target.value)}
                placeholder="Заголовок переписанной новости"
              />
            </S.Field>

            <S.Field>
              Краткое описание
              <S.TextArea
                value={form.summary}
                onChange={(event) => handleFieldChange('summary', event.target.value)}
                placeholder="Краткое описание (необязательно)"
                rows={3}
              />
            </S.Field>

            <S.Field>
              Текст новости
              <S.ContentArea
                value={form.content}
                onChange={(event) => handleFieldChange('content', event.target.value)}
                placeholder="Полный текст переписанной новости"
              />
            </S.Field>

            {error && <S.Error>{error}</S.Error>}
            {successMessage && <S.Success>{successMessage}</S.Success>}
          </S.Body>
        )}

        <S.Footer>
          <S.AiButton type="button" onClick={handleAiRewrite}>
            Переписать с помощью ИИ
          </S.AiButton>

          <S.FooterGroup>
            <S.SecondaryButton type="button" disabled={saving} onClick={onClose}>
              Отмена
            </S.SecondaryButton>
            <S.PrimaryButton
              type="button"
              disabled={loading || saving}
              onClick={() => void handleSave()}
            >
              {saving ? 'Сохранение…' : 'Сохранить'}
            </S.PrimaryButton>
          </S.FooterGroup>
        </S.Footer>
      </S.Dialog>
    </S.Overlay>
  );
}
