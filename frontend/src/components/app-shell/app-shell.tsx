import type { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { USER_ROLE_LABELS } from '../../api/role-labels';
import { PATHS } from '../../app/paths';
import { useAuth } from '../../contexts/auth-context';
import { AppNav } from '../app-nav/app-nav';
import type { AppSection } from '../app-nav/app-nav';
import * as S from './app-shell.styles';

interface AppShellProps {
  section: AppSection;
  sectionTitle: string;
  sectionSubtitle: string;
  children: ReactNode;
}

export function AppShell({ section, sectionTitle, sectionSubtitle, children }: AppShellProps) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate(PATHS.login);
  };

  return (
    <S.Shell>
      <S.Sidebar>
        <S.Brand>
          <S.BrandTitle>News Collector</S.BrandTitle>
          <S.BrandSubtitle>Сбор и переписывание новостей</S.BrandSubtitle>
        </S.Brand>
        <S.SidebarNavSlot>
          <AppNav section={section} variant="sidebar" />
        </S.SidebarNavSlot>
        {user && (
          <S.UserPanel>
            <S.UserName>{user.displayName}</S.UserName>
            <S.UserRole>{USER_ROLE_LABELS[user.role]}</S.UserRole>
            <S.LogoutButton type="button" onClick={handleLogout}>
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
            <S.MobileLogout type="button" onClick={handleLogout}>
              Выйти
            </S.MobileLogout>
          )}
        </S.MobileTopBar>

        <S.Content>
          <S.PageHeader>
            <S.PageTitle>{sectionTitle}</S.PageTitle>
            <S.PageSubtitle>{sectionSubtitle}</S.PageSubtitle>
          </S.PageHeader>

          <S.PageBody>{children}</S.PageBody>
        </S.Content>

        <AppNav section={section} variant="bottom" />
      </S.Main>
    </S.Shell>
  );
}