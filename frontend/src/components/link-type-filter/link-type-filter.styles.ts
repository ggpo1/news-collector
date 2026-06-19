import styled, { css } from 'styled-components';

export const Filters = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-bottom: 1rem;
`;

export const FilterButton = styled.button<{ $active?: boolean }>`
  border: 1px solid ${({ theme }) => theme.colors.border};
  min-height: 2.5rem;
  padding: 0.45rem 0.95rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  font-size: 0.82rem;
  font-weight: 600;
  cursor: pointer;
  color: ${({ theme }) => theme.colors.textMuted};
  background: ${({ theme }) => theme.colors.surface};
  transition: border-color 0.15s, color 0.15s, background 0.15s;

  &:hover {
    border-color: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 50%, ${({ theme }) => theme.colors.border});
    color: ${({ theme }) => theme.colors.text};
  }

  ${({ theme, $active }) =>
    $active &&
    css`
      border-color: ${theme.colors.accent};
      color: ${theme.colors.accent};
      background: color-mix(in srgb, ${theme.colors.accent} 12%, transparent);
    `}
`;
