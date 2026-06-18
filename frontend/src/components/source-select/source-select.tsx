import type { Source } from '../../api/types';
import * as S from './source-select.styles';

interface SourceSelectProps {
  sources: Source[];
  value: string | null;
  loading: boolean;
  onChange: (sourceId: string) => void;
}

export function SourceSelect({ sources, value, loading, onChange }: SourceSelectProps) {
  return (
    <S.Root>
      <S.Label>Источник</S.Label>
      <S.Select
        value={value ?? ''}
        disabled={loading || sources.length === 0}
        onChange={(e) => onChange(e.target.value)}
      >
        {sources.length === 0 ? (
          <option value="">Нет источников</option>
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
