import type { ReactNode } from 'react';
import * as S from './empty-state.styles';

interface EmptyStateProps {
  children: ReactNode;
  error?: boolean;
}

export function EmptyState({ children, error = false }: EmptyStateProps) {
  return <S.Root $error={error}>{children}</S.Root>;
}
