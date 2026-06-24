import styled from 'styled-components';

export const Root = styled.div`
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
`;

export const Section = styled.section`
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
`;

export const SectionHeader = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  flex-wrap: wrap;
`;

export const SectionTitle = styled.h2`
  margin: 0;
  font-size: 1rem;
`;

export const AddButton = styled.button`
  border: 1px solid color-mix(in srgb, ${({ theme }) => theme.colors.accent} 45%, ${({ theme }) => theme.colors.border});
  background: ${({ theme }) => theme.colors.accentMuted};
  color: ${({ theme }) => theme.colors.accent};
  border-radius: ${({ theme }) => theme.radii.md};
  padding: 0.45rem 0.85rem;
  font-weight: 600;
  cursor: pointer;
`;

export const List = styled.ul`
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
`;

export const Card = styled.li`
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surface};
  padding: 0.85rem 1rem;
`;

export const CardTop = styled.div`
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 0.75rem;
`;

export const CardTitle = styled.h3`
  margin: 0;
  font-size: 0.98rem;
`;

export const CardMeta = styled.div`
  margin-top: 0.35rem;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem 0.75rem;
`;

export const Badge = styled.span<{ $variant?: 'ok' | 'warn' | 'muted' }>`
  padding: 0.1rem 0.45rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  font-size: 0.7rem;
  font-weight: 600;
  background: ${({ theme, $variant }) =>
    $variant === 'ok'
      ? `color-mix(in srgb, ${theme.colors.accent} 18%, ${theme.colors.surface})`
      : $variant === 'warn'
        ? `color-mix(in srgb, ${theme.colors.danger} 12%, ${theme.colors.surface})`
        : theme.colors.surfaceMuted};
  color: ${({ theme, $variant }) =>
    $variant === 'ok' ? theme.colors.accent : $variant === 'warn' ? theme.colors.danger : theme.colors.textMuted};
`;

export const Actions = styled.div`
  display: flex;
  gap: 0.4rem;
  flex-shrink: 0;
`;

export const ActionButton = styled.button`
  border: 1px solid ${({ theme }) => theme.colors.border};
  background: ${({ theme }) => theme.colors.surface};
  color: inherit;
  border-radius: ${({ theme }) => theme.radii.sm};
  padding: 0.3rem 0.55rem;
  font-size: 0.78rem;
  cursor: pointer;
`;

export const DangerButton = styled(ActionButton)`
  color: ${({ theme }) => theme.colors.danger};
`;

export const Routing = styled.div`
  margin-top: 0.45rem;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Hint = styled.p`
  margin: 0;
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.textMuted};
  line-height: 1.5;
`;
