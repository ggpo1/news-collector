import styled, { css } from 'styled-components';
import { NavLink } from 'react-router-dom';
import { mediaDown, mediaUp } from '../../styles/media';

export const SidebarNav = styled.nav`
  display: none;
  flex-direction: column;
  gap: 0.35rem;
  padding: 0.75rem;

  ${mediaUp('lg')} {
    display: flex;
  }
`;

export const BottomNav = styled.nav<{ $columns: number }>`
  position: fixed;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 100;
  display: grid;
  grid-template-columns: repeat(${({ $columns }) => $columns}, 1fr);
  gap: 0.25rem;
  padding: 0.35rem 0.5rem calc(0.35rem + env(safe-area-inset-bottom, 0px));
  border-top: 1px solid ${({ theme }) => theme.colors.borderSubtle};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.bgElevated} 92%, transparent);
  backdrop-filter: blur(16px);

  ${mediaUp('lg')} {
    display: none;
  }
`;

export const NavLinkButton = styled(NavLink)<{ $variant: 'sidebar' | 'bottom' }>`
  border: none;
  cursor: pointer;
  color: ${({ theme }) => theme.colors.textMuted};
  background: transparent;
  text-decoration: none;
  transition:
    background ${({ theme }) => theme.transitions.fast},
    color ${({ theme }) => theme.transitions.fast},
    transform ${({ theme }) => theme.transitions.fast};

  svg {
    width: 1.35rem;
    height: 1.35rem;
    display: block;
  }

  ${({ $variant }) =>
    $variant === 'sidebar'
      ? css`
          display: flex;
          align-items: center;
          gap: 0.75rem;
          width: 100%;
          padding: 0.7rem 0.85rem;
          border-radius: ${({ theme }) => theme.radii.md};
          font-size: 0.95rem;
          font-weight: 600;
          text-align: left;

          &:hover {
            color: ${({ theme }) => theme.colors.text};
            background: ${({ theme }) => theme.colors.surfaceHover};
          }
        `
      : css`
          display: flex;
          flex-direction: column;
          align-items: center;
          justify-content: center;
          gap: 0.2rem;
          min-height: 3.25rem;
          padding: 0.35rem 0.25rem;
          border-radius: ${({ theme }) => theme.radii.md};
          font-size: 0.68rem;
          font-weight: 600;
          line-height: 1.1;

          &:active {
            transform: scale(0.97);
          }
        `}

  &[aria-current='page'] {
    color: ${({ theme }) => theme.colors.accent};
    background: ${({ theme }) => theme.colors.accentMuted};
  }
`;

export const NavLabel = styled.span<{ $hideOnMobile?: boolean }>`
  ${mediaDown('sm')} {
    ${({ $hideOnMobile }) =>
      $hideOnMobile &&
      css`
        position: absolute;
        width: 1px;
        height: 1px;
        padding: 0;
        margin: -1px;
        overflow: hidden;
        clip: rect(0, 0, 0, 0);
        white-space: nowrap;
        border: 0;
      `}
  }
`;
