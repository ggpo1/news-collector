import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getStoryById, updateStoryStatus } from '../../api/client';
import { STORY_STATUS_LABELS, STORY_STATUS_OPTIONS } from '../../api/story-labels';
import { formatToneCoefficient } from '../../api/tone';
import type { StoryDetail, StoryStatus } from '../../api/types';
import { buildNewsPath } from '../../app/news-route';
import { EmptyState } from '../ui/empty-state';
import { LoadingState } from '../ui/loading-state';
import * as S from './story-detail.styles';

interface StoryDetailViewProps {
  storyId: string | null;
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

export function StoryDetailView({ storyId }: StoryDetailViewProps) {
  const navigate = useNavigate();
  const [story, setStory] = useState<StoryDetail | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [statusSaving, setStatusSaving] = useState(false);

  const load = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      const data = await getStoryById(id);
      setStory(data);
    } catch (err) {
      setStory(null);
      setError(err instanceof Error ? err.message : 'Не удалось загрузить тему');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!storyId) {
      setStory(null);
      setError(null);
      return;
    }

    void load(storyId);
  }, [storyId, load]);

  const handleStatusChange = async (status: StoryStatus) => {
    if (!story) {
      return;
    }

    setStatusSaving(true);
    try {
      const updated = await updateStoryStatus(story.id, status);
      setStory(updated);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось обновить статус');
    } finally {
      setStatusSaving(false);
    }
  };

  if (!storyId) {
    return (
      <S.Panel>
        <S.Placeholder>Выберите тему из списка</S.Placeholder>
      </S.Panel>
    );
  }

  if (loading) {
    return (
      <S.Panel>
        <LoadingState />
      </S.Panel>
    );
  }

  if (error || !story) {
    return (
      <S.Panel>
        <EmptyState error>{error ?? 'Тема не найдена'}</EmptyState>
      </S.Panel>
    );
  }

  return (
    <S.Panel>
      <S.Header>
        <S.Meta>
          <S.Badge>{STORY_STATUS_LABELS[story.status]}</S.Badge>
          <span>{story.sourceCount} источников</span>
          <span>{story.articleCount} материалов</span>
          <span>активность {formatDate(story.lastActivityAt)}</span>
        </S.Meta>
        <S.Title>{story.title}</S.Title>
        <S.StatusSelect
          value={story.status}
          disabled={statusSaving}
          onChange={(e) => void handleStatusChange(e.target.value as StoryStatus)}
        >
          {STORY_STATUS_OPTIONS.filter((option) => option.value !== '').map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </S.StatusSelect>
      </S.Header>

      <S.Section>
        <S.SectionTitle>Таймлайн</S.SectionTitle>
        <S.Timeline>
          {story.timeline.map((item) => (
            <S.TimelineItem key={item.id}>
              <S.ItemTitle>
                {item.isPrimary ? '★ ' : ''}
                {item.title}
              </S.ItemTitle>
              <S.ItemMeta>
                {item.sourceName}
                {item.categoryName ? ` · ${item.categoryName}` : ''}
                {formatToneCoefficient(item.toneCoefficient) ? ` · тон ${formatToneCoefficient(item.toneCoefficient)}` : ''}
                {' · '}
                <time dateTime={item.publishedAt ?? undefined}>{formatDate(item.publishedAt ?? item.fetchedAt)}</time>
                {' · '}
                <button
                  type="button"
                  onClick={() =>
                    navigate(
                      buildNewsPath({
                        page: 1,
                        sourceId: null,
                        categoryId: null,
                        uncategorized: false,
                        tone: null,
                        newsId: item.id,
                      }),
                    )
                  }
                >
                  открыть
                </button>
              </S.ItemMeta>
            </S.TimelineItem>
          ))}
        </S.Timeline>
      </S.Section>

      {story.keyEntities.length > 0 && (
        <S.Section>
          <S.SectionTitle>Ключевые сущности</S.SectionTitle>
          <S.ChipList>
            {story.keyEntities.map((entity) => (
              <S.Chip key={entity.id}>
                {entity.name} ({entity.mentionCount})
              </S.Chip>
            ))}
          </S.ChipList>
        </S.Section>
      )}

      {story.rewrites.length > 0 && (
        <S.Section>
          <S.SectionTitle>Переписи</S.SectionTitle>
          <S.SimpleList>
            {story.rewrites.map((rewrite) => (
              <li key={rewrite.id}>
                {rewrite.title} — {rewrite.authorName}
              </li>
            ))}
          </S.SimpleList>
        </S.Section>
      )}

      {story.telegramDeliveries.length > 0 && (
        <S.Section>
          <S.SectionTitle>Telegram</S.SectionTitle>
          <S.SimpleList>
            {story.telegramDeliveries.map((delivery) => (
              <li key={delivery.id}>
                {delivery.channelName}: {delivery.status} ({formatDate(delivery.sentAt ?? delivery.createdAt)})
              </li>
            ))}
          </S.SimpleList>
        </S.Section>
      )}
    </S.Panel>
  );
}
