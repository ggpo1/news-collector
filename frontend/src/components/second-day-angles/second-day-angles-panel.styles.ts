import styled from 'styled-components';

export const Panel = styled.div`
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
`;

export const Section = styled.section`
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
`;

export const SectionTitle = styled.h3`
  margin: 0;
  font-size: 0.95rem;
  color: ${({ theme }) => theme.colors.text};
`;

export const Empty = styled.p`
  margin: 0;
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const AngleList = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
`;

export const AngleCard = styled.article`
  padding: 0.85rem 1rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surfaceMuted};
`;

export const AngleHeader = styled.div`
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 0.75rem;
  margin-bottom: 0.45rem;
`;

export const AngleTitle = styled.h4`
  margin: 0;
  font-size: 0.92rem;
  line-height: 1.4;
`;

export const AngleBadge = styled.span`
  flex-shrink: 0;
  padding: 0.1rem 0.45rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  background: ${({ theme }) => theme.colors.accentMuted};
  color: ${({ theme }) => theme.colors.accent};
  font-size: 0.68rem;
  font-weight: 700;
  text-transform: uppercase;
`;

export const AngleRationale = styled.p`
  margin: 0;
  font-size: 0.84rem;
  line-height: 1.55;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const ItemList = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.55rem;
`;

export const ItemCard = styled.article`
  padding: 0.75rem 0.85rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surface};
`;

export const ItemHeader = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  margin-bottom: 0.25rem;
`;

export const ItemMeta = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-bottom: 0.3rem;
  font-size: 0.72rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const ItemTitle = styled.p`
  margin: 0 0 0.35rem;
  font-size: 0.88rem;
  font-weight: 600;
  line-height: 1.4;
`;

export const ItemText = styled.p`
  margin: 0;
  font-size: 0.84rem;
  line-height: 1.5;
`;

export const ItemHint = styled.p`
  margin: 0.45rem 0 0;
  font-size: 0.8rem;
  line-height: 1.45;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const RoleBadge = styled.span<{ $role: string }>`
  flex-shrink: 0;
  padding: 0.1rem 0.45rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  font-size: 0.68rem;
  font-weight: 700;
  text-transform: lowercase;
  color: ${({ theme, $role }) =>
    $role === 'beneficiary' ? theme.colors.success : $role === 'affected' ? theme.colors.danger : theme.colors.textMuted};
  background: ${({ theme, $role }) =>
    $role === 'beneficiary'
      ? 'color-mix(in srgb, #3dd68c 16%, transparent)'
      : $role === 'affected'
        ? 'color-mix(in srgb, #ff6b7a 14%, transparent)'
        : theme.colors.surfaceMuted};
`;

export const ValuesList = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  margin: 0.35rem 0;
`;

export const ValueRow = styled.div`
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  font-size: 0.84rem;

  strong {
    color: ${({ theme }) => theme.colors.accent};
    font-variant-numeric: tabular-nums;
  }
`;
