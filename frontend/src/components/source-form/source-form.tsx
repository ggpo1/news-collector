import { useEffect, useState } from 'react';
import { createSource, updateSource } from '../../api/client';
import type { CreateSourcePayload, Source, SourceType } from '../../api/types';
import * as S from './source-form.styles';

const SOURCE_TYPES: { value: SourceType; label: string }[] = [
  { value: 'Rss', label: 'RSS' },
  { value: 'Html', label: 'HTML' },
  { value: 'Api', label: 'API' },
];

interface SourceFormProps {
  source?: Source | null;
  onClose: () => void;
  onSaved: () => void;
}

interface FormState {
  name: string;
  type: SourceType;
  url: string;
  isActive: boolean;
  fetchIntervalMinutes: number;
  contentFetchEnabled: boolean;
  contentSelector: string;
}

function buildInitialForm(source?: Source | null): FormState {
  if (source) {
    return {
      name: source.name,
      type: normalizeSourceType(source.type),
      url: source.url,
      isActive: source.isActive,
      fetchIntervalMinutes: source.fetchIntervalMinutes,
      contentFetchEnabled: source.contentFetchEnabled,
      contentSelector: source.contentSelector ?? '',
    };
  }

  return {
    name: '',
    type: 'Rss',
    url: '',
    isActive: true,
    fetchIntervalMinutes: 15,
    contentFetchEnabled: true,
    contentSelector: '',
  };
}

function normalizeSourceType(type: SourceType | number): SourceType {
  if (type === 'Rss' || type === 'Html' || type === 'Api') {
    return type;
  }

  if (typeof type === 'number') {
    return (['Rss', 'Html', 'Api'][type] as SourceType | undefined) ?? 'Rss';
  }

  return 'Rss';
}

function toPayload(form: FormState): CreateSourcePayload {
  return {
    name: form.name.trim(),
    type: form.type,
    url: form.url.trim(),
    isActive: form.isActive,
    fetchIntervalMinutes: form.fetchIntervalMinutes,
    contentFetchEnabled: form.contentFetchEnabled,
    contentSelector: form.contentSelector.trim() || null,
  };
}

export function SourceForm({ source = null, onClose, onSaved }: SourceFormProps) {
  const isEdit = Boolean(source);
  const [form, setForm] = useState<FormState>(() => buildInitialForm(source));
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setForm(buildInitialForm(source));
    setError(null);
  }, [source]);

  const handleSubmit = async () => {
    const payload = toPayload(form);

    if (!payload.name) {
      setError('Укажите название источника');
      return;
    }

    if (!payload.url) {
      setError('Укажите URL источника');
      return;
    }

    if (payload.fetchIntervalMinutes < 1) {
      setError('Интервал опроса должен быть не меньше 1 минуты');
      return;
    }

    setSaving(true);
    setError(null);

    try {
      if (isEdit && source) {
        await updateSource(source.id, payload);
      } else {
        await createSource(payload);
      }

      onSaved();
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось сохранить источник');
    } finally {
      setSaving(false);
    }
  };

  return (
    <S.Overlay role="presentation" onClick={onClose}>
      <S.Dialog
        role="dialog"
        aria-modal="true"
        aria-labelledby="source-form-title"
        onClick={(event) => event.stopPropagation()}
      >
        <S.Header>
          <S.Title id="source-form-title">
            {isEdit ? 'Редактировать источник' : 'Добавить источник'}
          </S.Title>
          <S.CloseButton type="button" aria-label="Закрыть" disabled={saving} onClick={onClose}>
            ×
          </S.CloseButton>
        </S.Header>

        <S.Body>
          <S.Field>
            Название
            <S.Input
              value={form.name}
              disabled={saving}
              placeholder="Например, РИА Новости"
              onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))}
            />
          </S.Field>

          <S.Field>
            Тип
            <S.Select
              value={form.type}
              disabled={saving}
              onChange={(event) =>
                setForm((current) => ({ ...current, type: event.target.value as SourceType }))
              }
            >
              {SOURCE_TYPES.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </S.Select>
          </S.Field>

          <S.Field>
            URL
            <S.Input
              value={form.url}
              disabled={saving}
              placeholder="https://example.com/rss.xml"
              onChange={(event) => setForm((current) => ({ ...current, url: event.target.value }))}
            />
          </S.Field>

          <S.Field>
            Интервал опроса (мин)
            <S.Input
              type="number"
              min={1}
              value={form.fetchIntervalMinutes}
              disabled={saving}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  fetchIntervalMinutes: Number(event.target.value) || 1,
                }))
              }
            />
          </S.Field>

          <S.CheckboxRow>
            <input
              type="checkbox"
              checked={form.isActive}
              disabled={saving}
              onChange={(event) =>
                setForm((current) => ({ ...current, isActive: event.target.checked }))
              }
            />
            Источник активен
          </S.CheckboxRow>

          <S.CheckboxRow>
            <input
              type="checkbox"
              checked={form.contentFetchEnabled}
              disabled={saving}
              onChange={(event) =>
                setForm((current) => ({ ...current, contentFetchEnabled: event.target.checked }))
              }
            />
            Загружать полный текст статей
          </S.CheckboxRow>

          {form.contentFetchEnabled && (
            <S.Field>
              CSS-селектор контента
              <S.Input
                value={form.contentSelector}
                disabled={saving}
                placeholder=".article__text"
                onChange={(event) =>
                  setForm((current) => ({ ...current, contentSelector: event.target.value }))
                }
              />
            </S.Field>
          )}

          {error && <S.Error>{error}</S.Error>}
        </S.Body>

        <S.Footer>
          <S.SecondaryButton type="button" disabled={saving} onClick={onClose}>
            Отмена
          </S.SecondaryButton>
          <S.PrimaryButton type="button" disabled={saving} onClick={() => void handleSubmit()}>
            {saving ? 'Сохранение…' : isEdit ? 'Сохранить' : 'Добавить'}
          </S.PrimaryButton>
        </S.Footer>
      </S.Dialog>
    </S.Overlay>
  );
}
