import type { ReactNode } from 'react';
import { useMediaQuery } from '../../hooks/use-media-query/use-media-query';
import * as S from './master-detail-layout.styles';

interface MasterDetailLayoutProps {
  list: ReactNode;
  detail: ReactNode;
  detailOpen: boolean;
  onBack: () => void;
  backLabel?: string;
}

export function MasterDetailLayout({
  list,
  detail,
  detailOpen,
  onBack,
  backLabel = 'Назад к списку',
}: MasterDetailLayoutProps) {
  const isDesktop = useMediaQuery('lg');

  if (isDesktop) {
    return (
      <S.DesktopGrid>
        <S.ListColumn>{list}</S.ListColumn>
        <S.DetailColumn>{detail}</S.DetailColumn>
      </S.DesktopGrid>
    );
  }

  if (detailOpen) {
    return (
      <S.MobileDetail>
        <S.BackBar type="button" onClick={onBack}>
          <S.BackIcon aria-hidden="true">←</S.BackIcon>
          {backLabel}
        </S.BackBar>
        <S.MobileDetailBody>{detail}</S.MobileDetailBody>
      </S.MobileDetail>
    );
  }

  return <S.ListColumn>{list}</S.ListColumn>;
}
