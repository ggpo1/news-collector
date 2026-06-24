import type { EditorialTag } from '../../api/types';
import * as S from '../source-select/source-select.styles';

interface EditorialTagSelectProps {
  tags: EditorialTag[];
  value: string | null;
  loading: boolean;
  onChange: (value: string | null) => void;
}

export function EditorialTagSelect({ tags, value, loading, onChange }: EditorialTagSelectProps) {
  return (
    <S.Root>
      <S.Label>Редакционный тег</S.Label>
      <S.Select
        value={value ?? ''}
        disabled={loading}
        onChange={(e) => {
          const next = e.target.value;
          onChange(next === '' ? null : next);
        }}
      >
        <option value="">Все теги</option>
        {tags.map((tag) => (
          <option key={tag.id} value={tag.id}>
            {tag.name}
          </option>
        ))}
      </S.Select>
    </S.Root>
  );
}
