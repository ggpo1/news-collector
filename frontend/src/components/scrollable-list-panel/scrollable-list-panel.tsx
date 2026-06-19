import type { ReactNode } from 'react';
import * as S from './scrollable-list-panel.styles';

interface ScrollableListPanelProps {
  header?: ReactNode;
  footer?: ReactNode;
  children: ReactNode;
}

export function ScrollableListPanel({ header, footer, children }: ScrollableListPanelProps) {
  return (
    <S.Root>
      {header && <S.Header>{header}</S.Header>}
      <S.Body>{children}</S.Body>
      {footer && <S.Footer>{footer}</S.Footer>}
    </S.Root>
  );
}
