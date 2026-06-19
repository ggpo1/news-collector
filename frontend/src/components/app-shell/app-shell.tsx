import type { ReactNode } from 'react';
import { USER_ROLE_LABELS } from '../../api/role-labels';
import { useAuth } from '../../contexts/auth-context';
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
  const { user, logout } = useAuth();

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
        {user && (
          <S.UserPanel>
            <S.UserName>{user.displayName}</S.UserName>
            <S.UserRole>{USER_ROLE_LABELS[user.role]}</S.UserRole>
            <S.LogoutButton type="button" onClick={logout}>
              Выйти
            </S.LogoutButton>
          </S.UserPanel>
        )}
      </S.Sidebar>

      <S.Main>
        <S.MobileTopBar>
          <S.MobileTitle>
            <S.MobileHeading>{sectionTitle}</S.MobileHeading>
            <S.MobileSubtitle>{sectionSubtitle}</S.MobileSubtitle>
          </S.MobileTitle>
          {user && (
            <S.MobileLogout type="button" onClick={logout}>
              Выйти
            </S.MobileLogout>
          )}
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
