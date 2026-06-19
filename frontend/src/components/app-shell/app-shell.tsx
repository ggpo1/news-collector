import type { ReactNode } from 'react';
import { AppNav } from '../app-nav/app-nav';
import type { AppSection } from '../app-nav/app-nav';
import * as S from './app-shell.styles';

interface AppShellProps {
  section: AppSection;
  sectionTitle: string;
  sectionSubtitle: string;
  onSectionChange: (section: AppSection) => void;
  toolbar?: ReactNode;
  error?: string | null;
  children: ReactNode;
}

export function AppShell({
  section,
  sectionTitle,
  sectionSubtitle,
  onSectionChange,
  toolbar,
  error,
  children,
}: AppShellProps) {
  return (
    <S.Shell>
      <S.Sidebar>
        <S.Brand>
          <S.BrandTitle>News Collector</S.BrandTitle>
          <S.BrandSubtitle>Сбор и переписывание новостей</S.BrandSubtitle>
        </S.Brand>
        <S.SidebarNavSlot>
          <AppNav value={section} onChange={onSectionChange} variant="sidebar" />
        </S.SidebarNavSlot>
      </S.Sidebar>

      <S.Main>
        <S.MobileTopBar>
          <S.MobileTitle>
            <S.MobileHeading>{sectionTitle}</S.MobileHeading>
            <S.MobileSubtitle>{sectionSubtitle}</S.MobileSubtitle>
          </S.MobileTitle>
        </S.MobileTopBar>

        <S.Content>
          <S.PageHeader>
            <S.PageTitle>{sectionTitle}</S.PageTitle>
            <S.PageSubtitle>{sectionSubtitle}</S.PageSubtitle>
          </S.PageHeader>

          {toolbar && <S.Toolbar>{toolbar}</S.Toolbar>}
          {error && <S.ErrorBanner role="alert">{error}</S.ErrorBanner>}
          {children}
        </S.Content>

        <AppNav value={section} onChange={onSectionChange} variant="bottom" />
      </S.Main>
    </S.Shell>
  );
}
