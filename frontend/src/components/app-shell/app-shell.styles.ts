import styled from 'styled-components';
import { mediaUp } from '../../styles/media';

export const Shell = styled.div`
  min-height: 100dvh;
  height: 100dvh;
  display: flex;
  overflow: hidden;
`;

export const Sidebar = styled.aside`
  display: none;
  flex-direction: column;
  width: ${({ theme }) => theme.layout.sidebarWidth};
  border-right: 1px solid ${({ theme }) => theme.colors.borderSubtle};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.bgElevated} 88%, transparent);
  backdrop-filter: blur(12px);
  position: sticky;
  top: 0;
  height: 100dvh;

  ${mediaUp('lg')} {
    display: flex;
  }
`;

export const Brand = styled.div`
  padding: 1.35rem 1.25rem 1rem;
  border-bottom: 1px solid ${({ theme }) => theme.colors.borderSubtle};
`;

export const BrandTitle = styled.h1`
  margin: 0;
  font-size: 1.05rem;
  font-weight: 700;
  letter-spacing: -0.02em;
`;

export const BrandSubtitle = styled.p`
  margin: 0.3rem 0 0;
  font-size: 0.78rem;
  line-height: 1.4;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const SidebarNavSlot = styled.div`
  flex: 1;
  padding-top: 0.35rem;
  overflow-y: auto;
`;

export const UserPanel = styled.div`
  margin-top: auto;
  padding: 1rem 1.1rem 1.2rem;
  border-top: 1px solid ${({ theme }) => theme.colors.borderSubtle};
`;

export const UserName = styled.p`
  margin: 0;
  font-size: 0.92rem;
  font-weight: 600;
`;

export const UserRole = styled.p`
  margin: 0.2rem 0 0.75rem;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const LogoutButton = styled.button`
  width: 100%;
  min-height: 2.5rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: transparent;
  color: ${({ theme }) => theme.colors.text};
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;

  &:hover {
    background: ${({ theme }) => theme.colors.surfaceHover};
  }
`;

export const MobileLogout = styled.button`
  flex-shrink: 0;
  min-height: 2.25rem;
  padding: 0.35rem 0.7rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.pill};
  background: transparent;
  color: ${({ theme }) => theme.colors.textMuted};
  font-size: 0.78rem;
  font-weight: 600;
  cursor: pointer;
`;

export const Main = styled.main`
  flex: 1;
  min-width: 0;
  min-height: 0;
  display: flex;
  flex-direction: column;
  padding-bottom: calc(${({ theme }) => theme.layout.bottomNavHeight} + env(safe-area-inset-bottom, 0px));

  ${mediaUp('lg')} {
    padding-bottom: 0;
  }
`;

export const MobileTopBar = styled.header`
  position: sticky;
  top: 0;
  z-index: 50;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  min-height: ${({ theme }) => theme.layout.headerHeight};
  padding: 0.65rem 1rem;
  border-bottom: 1px solid ${({ theme }) => theme.colors.borderSubtle};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.bgElevated} 90%, transparent);
  backdrop-filter: blur(16px);

  ${mediaUp('lg')} {
    display: none;
  }
`;

export const MobileTitle = styled.div`
  min-width: 0;
`;

export const MobileHeading = styled.h1`
  margin: 0;
  font-size: 1rem;
  font-weight: 700;
  letter-spacing: -0.02em;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
`;

export const MobileSubtitle = styled.p`
  margin: 0.15rem 0 0;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
`;

export const Content = styled.div`
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  width: 100%;
  max-width: ${({ theme }) => theme.layout.maxContentWidth};
  margin: 0 auto;
  padding: 1rem;

  ${mediaUp('md')} {
    padding: 1.25rem 1.5rem;
  }

  ${mediaUp('lg')} {
    padding: 1.5rem 2rem;
  }
`;

export const PageBody = styled.div`
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
`;

export const PageHeader = styled.header`
  display: none;
  flex-shrink: 0;
  margin-bottom: 1.25rem;

  ${mediaUp('lg')} {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 1rem;
  }
`;

export const PageHeaderText = styled.div`
  min-width: 0;
`;

export const PageTitle = styled.h2`
  margin: 0;
  font-size: 1.65rem;
  font-weight: 700;
  letter-spacing: -0.03em;
`;

export const PageSubtitle = styled.p`
  margin: 0.35rem 0 0;
  font-size: 0.95rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Toolbar = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin-bottom: 1rem;
`;

export const ErrorBanner = styled.div`
  margin-bottom: 1rem;
  padding: 0.8rem 1rem;
  border-radius: ${({ theme }) => theme.radii.md};
  border: 1px solid color-mix(in srgb, ${({ theme }) => theme.colors.danger} 35%, transparent);
  background: color-mix(in srgb, ${({ theme }) => theme.colors.danger} 10%, transparent);
  color: ${({ theme }) => theme.colors.danger};
  font-size: 0.9rem;
`;
