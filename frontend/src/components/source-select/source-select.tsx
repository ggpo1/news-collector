import type { Source } from '../../api/types';
import * as S from './source-select.styles';

interface SourceSelectProps {
  sources: Source[];
  value: string | null;
  loading: boolean;
  onChange: (sourceId: string | null) => void;
}

export function SourceSelect({ sources, value, loading, onChange }: SourceSelectProps) {
  return (
    <S.Root>
      <S.Label>Источник</S.Label>
      <S.Select
        value={value ?? ''}
        disabled={loading}
        onChange={(e) => {
          const next = e.target.value;
          onChange(next === '' ? null : next);
        }}
      >
        <option value="">Все источники</option>
        {sources.length === 0 ? (
          <option value="" disabled>
            Нет активных источников
          </option>
        ) : (
          sources.map((source) => (
            <option key={source.id} value={source.id}>
              {source.name}
            </option>
          ))
        )}
      </S.Select>
    </S.Root>
  );
}
