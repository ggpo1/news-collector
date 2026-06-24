import * as S from '../source-select/source-select.styles';
export type NewsToneFilterValue =
  | 'positive'
  | 'negative'
  | 'neutral'
  | 'strong'
  | 'unanalyzed';

interface ToneSelectProps {
  value: NewsToneFilterValue | null;
  onChange: (value: NewsToneFilterValue | null) => void;
}

const OPTIONS: { value: NewsToneFilterValue; label: string }[] = [
  { value: 'positive', label: 'Позитивная (≥ +0.30)' },
  { value: 'negative', label: 'Негативная (≤ −0.30)' },
  { value: 'neutral', label: 'Нейтральная' },
  { value: 'strong', label: 'Ярко окрашенная (|тон| ≥ 0.55)' },
  { value: 'unanalyzed', label: 'Без анализа тона' },
];

export function ToneSelect({ value, onChange }: ToneSelectProps) {
  return (
    <S.Root>
      <S.Label>Тональность</S.Label>
      <S.Select
        value={value ?? ''}
        onChange={(e) => {
          const next = e.target.value;
          onChange(next === '' ? null : (next as NewsToneFilterValue));
        }}
      >
        <option value="">Все тональности</option>
        {OPTIONS.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </S.Select>
    </S.Root>
  );
}
