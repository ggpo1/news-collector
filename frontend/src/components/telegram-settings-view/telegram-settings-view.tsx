import { useCallback, useEffect, useState } from 'react';
import {
  deleteTelegramBot,
  deleteTelegramChannel,
  getTelegramBots,
  getTelegramChannels,
  restartTelegramBot,
} from '../../api/client';
import type { TelegramBot, TelegramChannel } from '../../api/types';
import { useCategories } from '../../hooks/use-categories/use-categories';
import { useSources } from '../../hooks/use-sources/use-sources';
import { EmptyState } from '../ui/empty-state';
import { LoadingState } from '../ui/loading-state';
import { TelegramBotForm } from '../telegram-bot-form/telegram-bot-form';
import { TelegramChannelForm } from '../telegram-channel-form/telegram-channel-form';
import * as S from './telegram-settings-view.styles';

function containerBadge(status: string): 'ok' | 'warn' | 'muted' {
  if (status === 'Running') {
    return 'ok';
  }

  if (status === 'Error') {
    return 'warn';
  }

  return 'muted';
}

export function TelegramSettingsView() {
  const { sources } = useSources();
  const { categories } = useCategories();
  const [bots, setBots] = useState<TelegramBot[]>([]);
  const [channels, setChannels] = useState<TelegramChannel[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);

  const [botFormOpen, setBotFormOpen] = useState(false);
  const [editingBot, setEditingBot] = useState<TelegramBot | null>(null);
  const [channelFormOpen, setChannelFormOpen] = useState(false);
  const [editingChannel, setEditingChannel] = useState<TelegramChannel | null>(null);

  const reload = useCallback(() => setReloadKey((value) => value + 1), []);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      setLoading(true);
      setError(null);

      try {
        const [botsData, channelsData] = await Promise.all([getTelegramBots(), getTelegramChannels()]);
        if (!cancelled) {
          setBots(botsData);
          setChannels(channelsData);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Не удалось загрузить настройки Telegram');
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
  }, [reloadKey]);

  const handleDeleteBot = async (bot: TelegramBot) => {
    if (!window.confirm(`Удалить бота «${bot.name}» и остановить его контейнер?`)) {
      return;
    }

    try {
      await deleteTelegramBot(bot.id);
      reload();
    } catch (err) {
      window.alert(err instanceof Error ? err.message : 'Не удалось удалить бота');
    }
  };

  const handleRestartBot = async (bot: TelegramBot) => {
    try {
      await restartTelegramBot(bot.id);
      reload();
    } catch (err) {
      window.alert(err instanceof Error ? err.message : 'Не удалось перезапустить контейнер');
    }
  };

  const handleDeleteChannel = async (channel: TelegramChannel) => {
    if (!window.confirm(`Удалить канал «${channel.name}»?`)) {
      return;
    }

    try {
      await deleteTelegramChannel(channel.id);
      reload();
    } catch (err) {
      window.alert(err instanceof Error ? err.message : 'Не удалось удалить канал');
    }
  };

  if (loading) {
    return <LoadingState label="Загрузка настроек Telegram…" />;
  }

  if (error) {
    return <EmptyState error>{error}</EmptyState>;
  }

  return (
    <>
      <S.Root>
        <S.Section>
          <S.SectionHeader>
            <S.SectionTitle>Боты</S.SectionTitle>
            <S.AddButton
              type="button"
              onClick={() => {
                setEditingBot(null);
                setBotFormOpen(true);
              }}
            >
              + Создать бота
            </S.AddButton>
          </S.SectionHeader>
          <S.Hint>
            Каждый бот получает свой Docker-контейнер-воркер, который отправляет сообщения в привязанные каналы.
          </S.Hint>

          {bots.length === 0 ? (
            <EmptyState>Ботов пока нет. Создайте бота через @BotFather и добавьте токен здесь.</EmptyState>
          ) : (
            <S.List>
              {bots.map((bot) => (
                <S.Card key={bot.id}>
                  <S.CardTop>
                    <div>
                      <S.CardTitle>{bot.name}</S.CardTitle>
                      <S.CardMeta>
                        <S.Badge $variant={bot.isActive ? 'ok' : 'muted'}>
                          {bot.isActive ? 'активен' : 'выключен'}
                        </S.Badge>
                        <S.Badge $variant={containerBadge(bot.containerStatus)}>
                          контейнер: {bot.containerStatus}
                        </S.Badge>
                        <span>токен {bot.botTokenMasked}</span>
                        <span>каналов: {bot.channelCount}</span>
                      </S.CardMeta>
                      {bot.containerError && <S.Routing>Ошибка: {bot.containerError}</S.Routing>}
                    </div>
                    <S.Actions>
                      <S.ActionButton
                        type="button"
                        onClick={() => {
                          setEditingBot(bot);
                          setBotFormOpen(true);
                        }}
                      >
                        Изменить
                      </S.ActionButton>
                      <S.ActionButton type="button" onClick={() => void handleRestartBot(bot)}>
                        Перезапуск
                      </S.ActionButton>
                      <S.DangerButton type="button" onClick={() => void handleDeleteBot(bot)}>
                        Удалить
                      </S.DangerButton>
                    </S.Actions>
                  </S.CardTop>
                </S.Card>
              ))}
            </S.List>
          )}
        </S.Section>

        <S.Section>
          <S.SectionHeader>
            <S.SectionTitle>Каналы</S.SectionTitle>
            <S.AddButton
              type="button"
              disabled={bots.length === 0}
              onClick={() => {
                setEditingChannel(null);
                setChannelFormOpen(true);
              }}
            >
              + Добавить канал
            </S.AddButton>
          </S.SectionHeader>
          <S.Hint>
            Привяжите канал к боту и укажите фильтры по категориям/источникам — они используются при ручной отправке и
            подсказке каналов в карточке новости.
          </S.Hint>

          {channels.length === 0 ? (
            <EmptyState>Каналов пока нет. Добавьте бота, затем создайте канал с chat id.</EmptyState>
          ) : (
            <S.List>
              {channels.map((channel) => (
                <S.Card key={channel.id}>
                  <S.CardTop>
                    <div>
                      <S.CardTitle>{channel.name}</S.CardTitle>
                      <S.CardMeta>
                        <S.Badge $variant={channel.isActive ? 'ok' : 'muted'}>
                          {channel.isActive ? 'активен' : 'выключен'}
                        </S.Badge>
                        <span>бот: {channel.botName}</span>
                        <span>chat: {channel.chatId}</span>
                      </S.CardMeta>
                      <S.Routing>
                        {channel.categoryNames.length > 0
                          ? `Категории: ${channel.categoryNames.join(', ')}`
                          : 'Категории: любые'}
                        {' · '}
                        {channel.sourceNames.length > 0
                          ? `Источники: ${channel.sourceNames.join(', ')}`
                          : 'Источники: любые'}
                      </S.Routing>
                    </div>
                    <S.Actions>
                      <S.ActionButton
                        type="button"
                        onClick={() => {
                          setEditingChannel(channel);
                          setChannelFormOpen(true);
                        }}
                      >
                        Изменить
                      </S.ActionButton>
                      <S.DangerButton type="button" onClick={() => void handleDeleteChannel(channel)}>
                        Удалить
                      </S.DangerButton>
                    </S.Actions>
                  </S.CardTop>
                </S.Card>
              ))}
            </S.List>
          )}
        </S.Section>
      </S.Root>

      {botFormOpen && (
        <TelegramBotForm
          bot={editingBot}
          onClose={() => {
            setBotFormOpen(false);
            setEditingBot(null);
          }}
          onSaved={reload}
        />
      )}

      {channelFormOpen && (
        <TelegramChannelForm
          channel={editingChannel}
          bots={bots}
          categories={categories}
          sources={sources}
          onClose={() => {
            setChannelFormOpen(false);
            setEditingChannel(null);
          }}
          onSaved={reload}
        />
      )}
    </>
  );
}
