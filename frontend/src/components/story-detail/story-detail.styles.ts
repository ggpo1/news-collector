import styled from 'styled-components';

export const Panel = styled.div`
  display: flex;
  flex-direction: column;
  gap: 1rem;
  min-height: 0;
`;

export const Header = styled.header`
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
`;

export const Title = styled.h2`
  margin: 0;
  font-size: 1.2rem;
  line-height: 1.35;
`;

export const Meta = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.45rem;
  align-items: center;
  font-size: 0.82rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Badge = styled.span`
  display: inline-flex;
  padding: 0.15rem 0.45rem;
  border-radius: 999px;
  font-size: 0.72rem;
  font-weight: 600;
  background: ${({ theme }) => theme.colors.surfaceMuted};
`;

export const StatusSelect = styled.select`
  padding: 0.35rem 0.55rem;
  border-radius: ${({ theme }) => theme.radii.sm};
  border: 1px solid ${({ theme }) => theme.colors.border};
  background: ${({ theme }) => theme.colors.surface};
  font: inherit;
`;

export const Section = styled.section`
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
`;

export const SectionTitle = styled.h3`
  margin: 0;
  font-size: 0.95rem;
`;

export const Timeline = styled.ul`
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
`;

export const TimelineItem = styled.li`
  padding: 0.75rem 0.85rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surface};
`;

export const ItemTitle = styled.div`
  font-weight: 600;
  line-height: 1.35;
`;

export const ItemMeta = styled.div`
  margin-top: 0.35rem;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const ChipList = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
`;

export const Chip = styled.span`
  padding: 0.15rem 0.45rem;
  border-radius: 999px;
  font-size: 0.72rem;
  background: ${({ theme }) => theme.colors.surfaceMuted};
`;

export const SimpleList = styled.ul`
  margin: 0;
  padding-left: 1rem;
  font-size: 0.82rem;
  color: ${({ theme }) => theme.colors.text};
`;

export const ErrorText = styled.p`
  margin: 0;
  color: ${({ theme }) => theme.colors.danger};
`;

export const Placeholder = styled.p`
  margin: 0;
  color: ${({ theme }) => theme.colors.textMuted};
`;
