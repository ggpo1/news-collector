import { useEffect, useState } from 'react';
import {
  getTelegramChannels,
  getTelegramDelivery,
  sendNewsToTelegram,
  sendRewriteToTelegram,
} from '../../api/client';
import type { TelegramChannel } from '../../api/types';
import { LoadingState } from '../ui/loading-state';
import * as S from './send-to-telegram-modal.styles';

interface SendToTelegramModalProps {
  title: string;
  mode: 'news' | 'rewrite';
  targetId: string;
  sourceId?: string | null;
  categoryId?: string | null;
  onClose: () => void;
}

export function SendToTelegramModal({
  title,
  mode,
  targetId,
  sourceId,
  categoryId,
  onClose,
}: SendToTelegramModalProps) {
  const [channels, setChannels] = useState<TelegramChannel[]>([]);
  const [loading, setLoading] = useState(true);
  const [sendingId, setSendingId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const waitForDelivery = async (deliveryId: string) => {
    const deadline = Date.now() + 30_000;

    while (Date.now() < deadline) {
      const delivery = await getTelegramDelivery(deliveryId);

      if (delivery.status === 'Sent') {
        setSuccess(`Сообщение отправлено в «${delivery.channelName}»`);
        return;
      }

      if (delivery.status === 'Failed') {
        throw new Error(delivery.errorMessage ?? 'Telegram отклонил сообщение');
      }

      await new Promise((resolve) => setTimeout(resolve, 2000));
    }

    setSuccess('Сообщение в очереди. Проверьте канал через несколько секунд.');
  };

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      setLoading(true);
      setError(null);

      try {
        const data = await getTelegramChannels({
          sourceId: sourceId ?? undefined,
          categoryId: categoryId ?? undefined,
        });
        if (!cancelled) {
          setChannels(data);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Не удалось загрузить каналы');
          setChannels([]);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    void load();

    return () => {
      cancelled = true;
    };
  }, [sourceId, categoryId]);

  const handleSend = async (channel: TelegramChannel) => {
    setSendingId(channel.id);
    setError(null);
    setSuccess(null);

    try {
      const result =
        mode === 'news'
          ? await sendNewsToTelegram(targetId, channel.id)
          : await sendRewriteToTelegram(targetId, channel.id);

      await waitForDelivery(result.deliveryId);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось отправить');
    } finally {
      setSendingId(null);
    }
  };

  return (
    <S.Overlay onClick={onClose}>
      <S.Dialog onClick={(event) => event.stopPropagation()} role="dialog" aria-modal="true">
        <S.Header>
          <S.Title>Отправить в Telegram</S.Title>
          <S.CloseButton type="button" onClick={onClose} aria-label="Закрыть">
            ×
          </S.CloseButton>
        </S.Header>
        <S.Body>
          <S.Hint>
            {title}
            <br />
            Показаны каналы без фильтров и подходящие по источнику/категории.
          </S.Hint>

          {loading ? (
            <LoadingState label="Загрузка каналов…" />
          ) : channels.length === 0 ? (
            <S.Hint>Нет доступных каналов. Добавьте бота и канал в разделе «Telegram».</S.Hint>
          ) : (
            <S.ChannelList>
              {channels.map((channel) => (
                <li key={channel.id}>
                  <S.ChannelButton
                    type="button"
                    disabled={sendingId !== null}
                    onClick={() => void handleSend(channel)}
                  >
                    <S.ChannelName>{channel.name}</S.ChannelName>
                    <S.ChannelMeta>
                      Бот: {channel.botName} · Chat: {channel.chatId}
                      {channel.categoryNames.length > 0 && ` · Категории: ${channel.categoryNames.join(', ')}`}
                      {channel.sourceNames.length > 0 && ` · Источники: ${channel.sourceNames.join(', ')}`}
                    </S.ChannelMeta>
                    {sendingId === channel.id && ' · отправка…'}
                  </S.ChannelButton>
                </li>
              ))}
            </S.ChannelList>
          )}

          {error && <S.Error>{error}</S.Error>}
          {success && <S.Success>{success}</S.Success>}
        </S.Body>
      </S.Dialog>
    </S.Overlay>
  );
}
