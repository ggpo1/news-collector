import type { ReactNode } from 'react';
import { useMediaQuery } from '../../hooks/use-media-query/use-media-query';
import { ScrollableListPanel } from '../scrollable-list-panel/scrollable-list-panel';
import * as S from './master-detail-layout.styles';

interface MasterDetailLayoutProps {
  listBody: ReactNode;
  listHeader?: ReactNode;
  listFooter?: ReactNode;
  stickyList?: boolean;
  detail: ReactNode;
  detailOpen: boolean;
  onBack: () => void;
  backLabel?: string;
}

export function MasterDetailLayout({
  listBody,
  listHeader,
  listFooter,
  stickyList = false,
  detail,
  detailOpen,
  onBack,
  backLabel = 'Назад к списку',
}: MasterDetailLayoutProps) {
  const isDesktop = useMediaQuery('lg');
  const useStickyPanel = stickyList && (listHeader !== undefined || listFooter !== undefined);

  const listContent = useStickyPanel ? (
    <ScrollableListPanel header={listHeader} footer={listFooter}>
      {listBody}
    </ScrollableListPanel>
  ) : (
    <>
      {listHeader}
      {listBody}
      {listFooter}
    </>
  );

  if (isDesktop) {
    return (
      <S.DesktopGrid>
        <S.ListColumn $sticky={useStickyPanel}>{listContent}</S.ListColumn>
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

  return <S.ListColumn $sticky={useStickyPanel}>{listContent}</S.ListColumn>;
}
