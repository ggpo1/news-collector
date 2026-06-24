import { useState } from 'react';
import { createTelegramBot, updateTelegramBot } from '../../api/client';
import type { TelegramBot } from '../../api/types';
import * as S from '../source-form/source-form.styles';

interface TelegramBotFormProps {
  bot?: TelegramBot | null;
  onClose: () => void;
  onSaved: () => void;
}

export function TelegramBotForm({ bot = null, onClose, onSaved }: TelegramBotFormProps) {
  const isEdit = Boolean(bot);
  const [name, setName] = useState(bot?.name ?? '');
  const [botToken, setBotToken] = useState('');
  const [isActive, setIsActive] = useState(bot?.isActive ?? true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async () => {
    setSaving(true);
    setError(null);

    try {
      if (isEdit && bot) {
        await updateTelegramBot(bot.id, {
          name: name.trim(),
          botToken: botToken.trim() || null,
          isActive,
        });
      } else {
        if (!botToken.trim()) {
          throw new Error('Укажите токен бота от @BotFather');
        }

        await createTelegramBot({
          name: name.trim(),
          botToken: botToken.trim(),
          isActive,
        });
      }

      onSaved();
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось сохранить бота');
    } finally {
      setSaving(false);
    }
  };

  return (
    <S.Overlay role="presentation" onClick={onClose}>
      <S.Dialog role="dialog" aria-modal="true" onClick={(event) => event.stopPropagation()}>
        <S.Header>
          <S.Title>{isEdit ? 'Редактировать бота' : 'Создать бота'}</S.Title>
          <S.CloseButton type="button" onClick={onClose} aria-label="Закрыть">
            ×
          </S.CloseButton>
        </S.Header>

        <S.Body>
          {!isEdit && (
            <S.Error as="p" style={{ color: 'inherit', opacity: 0.8 }}>
              После создания API запустит отдельный Docker-контейнер воркера для этого бота.
            </S.Error>
          )}

          <S.Field>
            Название
            <S.Input value={name} onChange={(event) => setName(event.target.value)} disabled={saving} />
          </S.Field>

          <S.Field>
            Токен бота {isEdit && `(текущий: ${bot?.botTokenMasked})`}
            <S.Input
              type="password"
              value={botToken}
              onChange={(event) => setBotToken(event.target.value)}
              placeholder={isEdit ? 'Оставьте пустым, чтобы не менять' : '123456:ABC...'}
              disabled={saving}
            />
          </S.Field>

          <S.CheckboxRow>
            <input
              type="checkbox"
              checked={isActive}
              disabled={saving}
              onChange={(event) => setIsActive(event.target.checked)}
            />
            Бот активен
          </S.CheckboxRow>

          {error && <S.Error>{error}</S.Error>}
        </S.Body>

        <S.Footer>
          <S.SecondaryButton type="button" disabled={saving} onClick={onClose}>
            Отмена
          </S.SecondaryButton>
          <S.PrimaryButton type="button" disabled={saving} onClick={() => void handleSubmit()}>
            {saving ? 'Сохранение…' : isEdit ? 'Сохранить' : 'Создать бота'}
          </S.PrimaryButton>
        </S.Footer>
      </S.Dialog>
    </S.Overlay>
  );
}
