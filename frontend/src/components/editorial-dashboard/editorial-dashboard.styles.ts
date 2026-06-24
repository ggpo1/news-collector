import styled from 'styled-components';
import { mediaUp } from '../../styles/media';

export const Page = styled.div`
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
  min-height: 0;
`;

export const Toolbar = styled.div`
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
`;

export const ToolbarGroup = styled.div`
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.5rem;
`;

export const Select = styled.select`
  padding: 0.45rem 0.65rem;
  border-radius: ${({ theme }) => theme.radii.sm};
  border: 1px solid ${({ theme }) => theme.colors.border};
  background: ${({ theme }) => theme.colors.surface};
  color: inherit;
  font: inherit;
`;

export const RefreshButton = styled.button`
  padding: 0.45rem 0.85rem;
  border-radius: ${({ theme }) => theme.radii.sm};
  border: 1px solid ${({ theme }) => theme.colors.border};
  background: ${({ theme }) => theme.colors.surface};
  color: inherit;
  cursor: pointer;
  font: inherit;

  &:hover {
    border-color: ${({ theme }) => theme.colors.accent};
  }

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
`;

export const MetaLine = styled.p`
  margin: 0;
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Grid = styled.div`
  display: grid;
  gap: 1.25rem;

  ${mediaUp('lg')} {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
`;

export const Section = styled.section`
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  min-width: 0;
`;

export const TabbedSection = styled(Section)`
  ${mediaUp('lg')} {
    grid-row: span 2;
  }
`;

export const TabList = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
`;

export const TabButton = styled.button<{ $active?: boolean }>`
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.4rem 0.7rem;
  border-radius: ${({ theme }) => theme.radii.sm};
  border: 1px solid
    ${({ theme, $active }) =>
      $active ? theme.colors.accent : theme.colors.border};
  background: ${({ theme, $active }) =>
    $active
      ? `color-mix(in srgb, ${theme.colors.accent} 10%, ${theme.colors.surface})`
      : theme.colors.surface};
  color: ${({ theme, $active }) => ($active ? theme.colors.text : theme.colors.textMuted)};
  font: inherit;
  font-size: 0.9rem;
  font-weight: ${({ $active }) => ($active ? 700 : 500)};
  cursor: pointer;
  transition:
    color ${({ theme }) => theme.transitions.fast},
    border-color ${({ theme }) => theme.transitions.fast},
    background ${({ theme }) => theme.transitions.fast};

  &:hover {
    color: ${({ theme }) => theme.colors.text};
    border-color: ${({ theme }) => theme.colors.accent};
  }
`;

export const TabCount = styled.span`
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 1.25rem;
  padding: 0.05rem 0.3rem;
  border-radius: 999px;
  font-size: 0.7rem;
  font-weight: 700;
  font-variant-numeric: tabular-nums;
  background: ${({ theme }) => theme.colors.surfaceMuted};
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const SectionTitle = styled.h2`
  margin: 0;
  font-size: 1rem;
  font-weight: 700;
`;

export const SectionHint = styled.p`
  margin: -0.35rem 0 0;
  font-size: 0.8rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const CardList = styled.ul`
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
`;

export const Card = styled.button`
  width: 100%;
  text-align: left;
  padding: 0.9rem 1rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surface};
  color: inherit;
  cursor: pointer;
  transition: border-color ${({ theme }) => theme.transitions.fast};

  &:hover:not(:disabled) {
    border-color: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 45%, ${({ theme }) => theme.colors.border});
  }

  &:disabled {
    opacity: 0.65;
    cursor: default;
  }
`;

export const CardTitle = styled.div`
  font-weight: 600;
  line-height: 1.35;
  margin-bottom: 0.45rem;
`;

export const Badges = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
  margin-bottom: 0.45rem;
`;

export const Badge = styled.span<{ $variant?: 'hot' | 'dup' | 'tone' | 'entity' }>`
  display: inline-flex;
  align-items: center;
  padding: 0.15rem 0.45rem;
  border-radius: 999px;
  font-size: 0.72rem;
  font-weight: 600;
  background: ${({ theme, $variant }) => {
    switch ($variant) {
      case 'hot':
        return 'color-mix(in srgb, #e85d04 14%, transparent)';
      case 'dup':
        return 'color-mix(in srgb, #7209b7 14%, transparent)';
      case 'tone':
        return 'color-mix(in srgb, #2a9d8f 14%, transparent)';
      case 'entity':
        return 'color-mix(in srgb, #4361ee 14%, transparent)';
      default:
        return theme.colors.surfaceMuted;
    }
  }};
  color: ${({ theme }) => theme.colors.text};
`;

export const CardMeta = styled.div`
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
  line-height: 1.45;
`;

export const RelatedList = styled.ul`
  margin: 0.5rem 0 0;
  padding-left: 1rem;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};

  li + li {
    margin-top: 0.2rem;
  }
`;

export const EmptySection = styled.p`
  margin: 0;
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const ErrorText = styled.p`
  margin: 0;
  color: ${({ theme }) => theme.colors.danger};
  font-size: 0.9rem;
`;
