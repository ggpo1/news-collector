import type { LinkMethod, LinkType } from '../api/types';

export const LINK_TYPE_LABELS: Record<LinkType, string> = {
  SameTopic: 'Одна тема',
  Duplicate: 'Дубликат',
  Related: 'Связанная',
};

export const LINK_METHOD_LABELS: Record<LinkMethod, string> = {
  Manual: 'Вручную',
  TitleSimilarity: 'Схожесть заголовков',
  Embedding: 'Эмбеддинги',
};

export function formatConfidence(value: number): string {
  return `${Math.round(value * 100)}%`;
}
