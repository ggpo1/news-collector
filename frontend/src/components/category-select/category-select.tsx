import type { Category } from '../../api/types';
import * as S from '../source-select/source-select.styles';

export const UNCATEGORIZED_FILTER = '__uncategorized__';

interface CategorySelectProps {
  categories: Category[];
  value: string | null;
  loading: boolean;
  onChange: (value: string | null) => void;
}

export function CategorySelect({ categories, value, loading, onChange }: CategorySelectProps) {
  return (
    <S.Root>
      <S.Label>Категория</S.Label>
      <S.Select
        value={value ?? ''}
        disabled={loading}
        onChange={(e) => {
          const next = e.target.value;
          onChange(next === '' ? null : next);
        }}
      >
        <option value="">Все категории</option>
        <option value={UNCATEGORIZED_FILTER}>Без категории</option>
        {categories.map((category) => (
          <option key={category.id} value={category.id}>
            {category.name}
          </option>
        ))}
      </S.Select>
    </S.Root>
  );
}
