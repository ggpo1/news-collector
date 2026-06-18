import type { LinkType } from '../../api/types';
import { LINK_TYPE_LABELS } from '../../api/link-labels';
import * as S from './link-type-filter.styles';

const OPTIONS: Array<{ value: LinkType | null; label: string }> = [
  { value: null, label: 'Все' },
  { value: 'SameTopic', label: LINK_TYPE_LABELS.SameTopic },
  { value: 'Duplicate', label: LINK_TYPE_LABELS.Duplicate },
  { value: 'Related', label: LINK_TYPE_LABELS.Related },
];

interface LinkTypeFilterProps {
  value: LinkType | null;
  onChange: (value: LinkType | null) => void;
}

export function LinkTypeFilter({ value, onChange }: LinkTypeFilterProps) {
  return (
    <S.Filters role="group" aria-label="Тип связи">
      {OPTIONS.map((option) => (
        <S.FilterButton
          key={option.label}
          type="button"
          $active={value === option.value}
          onClick={() => onChange(option.value)}
        >
          {option.label}
        </S.FilterButton>
      ))}
    </S.Filters>
  );
}
