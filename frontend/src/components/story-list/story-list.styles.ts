import styled from 'styled-components';

export const List = styled.ul`
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
`;

export const Card = styled.button<{ $active?: boolean }>`
  width: 100%;
  text-align: left;
  padding: 0.9rem 1rem;
  border: 1px solid
    ${({ theme, $active }) =>
      $active
        ? `color-mix(in srgb, ${theme.colors.accent} 45%, ${theme.colors.border})`
        : theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surface};
  color: inherit;
  cursor: pointer;
`;

export const Title = styled.div`
  font-weight: 600;
  line-height: 1.35;
  margin-bottom: 0.45rem;
`;

export const Meta = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
  align-items: center;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Badge = styled.span<{ $status?: string }>`
  display: inline-flex;
  padding: 0.15rem 0.45rem;
  border-radius: 999px;
  font-size: 0.72rem;
  font-weight: 600;
  background: ${({ theme }) => theme.colors.surfaceMuted};
`;

export const Sources = styled.div`
  margin-top: 0.45rem;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;
