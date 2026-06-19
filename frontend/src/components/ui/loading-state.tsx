import * as S from './loading-state.styles';

interface LoadingStateProps {
  label?: string;
}

export function LoadingState({ label = 'Загрузка…' }: LoadingStateProps) {
  return (
    <S.Root role="status" aria-live="polite">
      <S.Spinner aria-hidden="true" />
      <S.Label>{label}</S.Label>
    </S.Root>
  );
}
