import styled, { css } from 'styled-components';

export const Nav = styled.nav`
  display: flex;
  gap: 0.35rem;
  padding: 0.25rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  background: ${({ theme }) => theme.colors.surface};
  border: 1px solid ${({ theme }) => theme.colors.border};
`;

export const NavButton = styled.button<{ $active?: boolean }>`
  border: none;
  padding: 0.45rem 1rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  color: ${({ theme }) => theme.colors.textMuted};
  background: transparent;
  transition: background 0.15s, color 0.15s;

  &:hover {
    color: ${({ theme }) => theme.colors.text};
  }

  ${({ theme, $active }) =>
    $active &&
    css`
      background: color-mix(in srgb, ${theme.colors.accent} 18%, transparent);
      color: ${theme.colors.accent};
    `}
`;
