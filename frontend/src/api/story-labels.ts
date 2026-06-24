import type { StoryStatus } from '../api/types';

export const STORY_STATUS_LABELS: Record<StoryStatus, string> = {
  Monitoring: 'Мониторим',
  InWork: 'В работе',
  Published: 'Опубликовано',
  Closed: 'Закрыто',
};

export const STORY_STATUS_OPTIONS: { value: StoryStatus | ''; label: string }[] = [
  { value: '', label: 'Все статусы' },
  { value: 'Monitoring', label: STORY_STATUS_LABELS.Monitoring },
  { value: 'InWork', label: STORY_STATUS_LABELS.InWork },
  { value: 'Published', label: STORY_STATUS_LABELS.Published },
  { value: 'Closed', label: STORY_STATUS_LABELS.Closed },
];
