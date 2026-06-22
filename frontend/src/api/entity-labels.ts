import type { NamedEntityType } from './entities';

const ENTITY_TYPE_LABELS: Record<NamedEntityType, string> = {
  Person: 'Персона',
  Company: 'Компания',
  Country: 'Страна',
  Organization: 'Организация',
  Location: 'Локация',
  Event: 'Событие',
  Other: 'Другое',
};

export function entityTypeLabel(type: NamedEntityType): string {
  return ENTITY_TYPE_LABELS[type] ?? type;
}

export const ENTITY_TYPE_OPTIONS: { value: NamedEntityType | ''; label: string }[] = [
  { value: '', label: 'Все типы' },
  { value: 'Person', label: 'Персоны' },
  { value: 'Company', label: 'Компании' },
  { value: 'Country', label: 'Страны' },
  { value: 'Organization', label: 'Организации' },
  { value: 'Location', label: 'Локации' },
  { value: 'Event', label: 'События' },
  { value: 'Other', label: 'Другое' },
];

export const ENTITY_TYPE_COLORS: Record<NamedEntityType, string> = {
  Person: '#5b9aff',
  Company: '#3dd68c',
  Country: '#ffb347',
  Organization: '#b794f6',
  Location: '#4ecdc4',
  Event: '#ff6b7a',
  Other: '#8b9cb3',
};
