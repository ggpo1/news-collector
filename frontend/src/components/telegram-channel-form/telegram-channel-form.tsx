import { useState } from 'react';
import { createTelegramChannel, updateTelegramChannel } from '../../api/client';
import type { Category, Source, TelegramBot, TelegramChannel } from '../../api/types';
import * as S from '../source-form/source-form.styles';

interface TelegramChannelFormProps {
  channel?: TelegramChannel | null;
  bots: TelegramBot[];
  categories: Category[];
  sources: Source[];
  onClose: () => void;
  onSaved: () => void;
}

export function TelegramChannelForm({
  channel = null,
  bots,
  categories,
  sources,
  onClose,
  onSaved,
}: TelegramChannelFormProps) {
  const isEdit = Boolean(channel);
  const [telegramBotId, setTelegramBotId] = useState(channel?.telegramBotId ?? bots[0]?.id ?? '');
  const [name, setName] = useState(channel?.name ?? '');
  const [chatId, setChatId] = useState(channel?.chatId ?? '');
  const [isActive, setIsActive] = useState(channel?.isActive ?? true);
  const [categoryIds, setCategoryIds] = useState<string[]>(channel?.categoryIds ?? []);
  const [sourceIds, setSourceIds] = useState<string[]>(channel?.sourceIds ?? []);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const toggleId = (list: string[], id: string, checked: boolean) =>
    checked ? [...list, id] : list.filter((item) => item !== id);

  const handleSubmit = async () => {
    if (!telegramBotId) {
      setError('Выберите бота');
      return;
    }

    setSaving(true);
    setError(null);

    const payload = {
      telegramBotId,
      name: name.trim(),
      chatId: chatId.trim(),
      isActive,
      categoryIds,
      sourceIds,
    };

    try {
      if (isEdit && channel) {
        await updateTelegramChannel(channel.id, payload);
      } else {
        await createTelegramChannel(payload);
      }

      onSaved();
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось сохранить канал');
    } finally {
      setSaving(false);
    }
  };

  return (
    <S.Overlay role="presentation" onClick={onClose}>
      <S.Dialog role="dialog" aria-modal="true" onClick={(event) => event.stopPropagation()}>
        <S.Header>
          <S.Title>{isEdit ? 'Редактировать канал' : 'Добавить канал'}</S.Title>
          <S.CloseButton type="button" onClick={onClose} aria-label="Закрыть">
            ×
          </S.CloseButton>
        </S.Header>

        <S.Body>
          <S.Field>
            Бот
            <S.Select
              value={telegramBotId}
              disabled={saving || bots.length === 0}
              onChange={(event) => setTelegramBotId(event.target.value)}
            >
              {bots.map((bot) => (
                <option key={bot.id} value={bot.id}>
                  {bot.name}
                </option>
              ))}
            </S.Select>
          </S.Field>

          <S.Field>
            Название канала
            <S.Input value={name} disabled={saving} onChange={(event) => setName(event.target.value)} />
          </S.Field>

          <S.Field>
            Chat ID
            <S.Input
              value={chatId}
              disabled={saving}
              placeholder="-1001234567890 или @mychannel"
              onChange={(event) => setChatId(event.target.value)}
            />
          </S.Field>

          <S.Field>
            Категории (пусто = любые)
            {categories.map((category) => (
              <S.CheckboxRow key={category.id}>
                <input
                  type="checkbox"
                  checked={categoryIds.includes(category.id)}
                  disabled={saving}
                  onChange={(event) =>
                    setCategoryIds((prev) => toggleId(prev, category.id, event.target.checked))
                  }
                />
                {category.name}
              </S.CheckboxRow>
            ))}
          </S.Field>

          <S.Field>
            Источники (пусто = любые)
            {sources.map((source) => (
              <S.CheckboxRow key={source.id}>
                <input
                  type="checkbox"
                  checked={sourceIds.includes(source.id)}
                  disabled={saving}
                  onChange={(event) =>
                    setSourceIds((prev) => toggleId(prev, source.id, event.target.checked))
                  }
                />
                {source.name}
              </S.CheckboxRow>
            ))}
          </S.Field>

          <S.CheckboxRow>
            <input
              type="checkbox"
              checked={isActive}
              disabled={saving}
              onChange={(event) => setIsActive(event.target.checked)}
            />
            Канал активен
          </S.CheckboxRow>

          {error && <S.Error>{error}</S.Error>}
        </S.Body>

        <S.Footer>
          <S.SecondaryButton type="button" disabled={saving} onClick={onClose}>
            Отмена
          </S.SecondaryButton>
          <S.PrimaryButton type="button" disabled={saving || bots.length === 0} onClick={() => void handleSubmit()}>
            {saving ? 'Сохранение…' : isEdit ? 'Сохранить' : 'Добавить канал'}
          </S.PrimaryButton>
        </S.Footer>
      </S.Dialog>
    </S.Overlay>
  );
}
