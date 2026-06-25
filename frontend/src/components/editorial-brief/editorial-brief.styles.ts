import styled from 'styled-components';

export const Page = styled.div`
  display: flex;
  flex-direction: column;
  gap: 1rem;
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

export const Button = styled.button`
  padding: 0.45rem 0.85rem;
  border-radius: ${({ theme }) => theme.radii.sm};
  border: 1px solid ${({ theme }) => theme.colors.border};
  background: ${({ theme }) => theme.colors.surface};
  color: inherit;
  cursor: pointer;
  font: inherit;

  &:hover:not(:disabled) {
    border-color: ${({ theme }) => theme.colors.accent};
  }

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
`;

export const PrimaryButton = styled(Button)`
  border-color: ${({ theme }) => theme.colors.accent};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 12%, ${({ theme }) => theme.colors.surface});
`;

export const MetaLine = styled.p`
  margin: 0;
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const ErrorText = styled.p`
  margin: 0;
  color: ${({ theme }) => theme.colors.danger};
  font-size: 0.9rem;
`;

export const EmptyState = styled.div`
  padding: 2rem 1rem;
  text-align: center;
  color: ${({ theme }) => theme.colors.textMuted};
  font-size: 0.95rem;
  line-height: 1.5;
`;

export const MarkdownBody = styled.article`
  padding: 1.15rem 1.25rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surface};
  line-height: 1.55;
  font-size: 0.95rem;

  h2 {
    margin: 1.25rem 0 0.65rem;
    font-size: 1.1rem;
    font-weight: 700;

    &:first-child {
      margin-top: 0;
    }
  }

  h3 {
    margin: 1rem 0 0.5rem;
    font-size: 1rem;
    font-weight: 700;
  }

  p {
    margin: 0.45rem 0;
  }

  ul {
    margin: 0.35rem 0 0.75rem;
    padding-left: 1.25rem;
  }

  li {
    margin: 0.25rem 0;
  }

  strong {
    font-weight: 700;
  }
`;

export const HistoryList = styled.ul`
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
`;

export const HistoryButton = styled.button<{ $active?: boolean }>`
  width: 100%;
  text-align: left;
  padding: 0.55rem 0.75rem;
  border-radius: ${({ theme }) => theme.radii.sm};
  border: 1px solid ${({ theme, $active }) => ($active ? theme.colors.accent : theme.colors.border)};
  background: ${({ theme, $active }) =>
    $active
      ? `color-mix(in srgb, ${theme.colors.accent} 8%, ${theme.colors.surface})`
      : theme.colors.surface};
  color: inherit;
  cursor: pointer;
  font: inherit;
  font-size: 0.85rem;
`;
