import styled from 'styled-components';

export const Panel = styled.aside`
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surface};
  padding: 1.25rem;
  min-height: 320px;
  position: sticky;
  top: 1rem;
`;

export const Placeholder = styled.div`
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 280px;
  color: ${({ theme }) => theme.colors.textMuted};
  text-align: center;
  padding: 1rem;
`;

export const Header = styled.header`
  margin-bottom: 1rem;
`;

export const Meta = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem 1rem;
  margin-bottom: 0.75rem;
  font-size: 0.82rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Title = styled.h2`
  margin: 0;
  font-size: 1.25rem;
  line-height: 1.35;
`;

export const Content = styled.div`
  font-size: 0.95rem;
  line-height: 1.65;
  color: ${({ theme }) => theme.colors.text};
  white-space: pre-wrap;
`;

export const ExternalLink = styled.a`
  font-size: 0.9rem;
  color: ${({ theme }) => theme.colors.accent};
  text-decoration: none;

  &:hover {
    text-decoration: underline;
  }
`;

export const State = styled.div<{ $error?: boolean }>`
  padding: 2rem 1rem;
  text-align: center;
  color: ${({ theme, $error }) => ($error ? theme.colors.danger : theme.colors.textMuted)};
`;

export const Actions = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  align-items: center;
  margin-top: 1rem;
`;

export const FetchButton = styled.button`
  padding: 0.5rem 1rem;
  border: 1px solid ${({ theme }) => theme.colors.accent};
  border-radius: ${({ theme }) => theme.radii.sm};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 12%, transparent);
  color: ${({ theme }) => theme.colors.accent};
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;

  &:hover:not(:disabled) {
    background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 22%, transparent);
  }

  &:disabled {
    opacity: 0.6;
    cursor: wait;
  }
`;

export const RewriteButton = styled.button`
  padding: 0.5rem 1rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.sm};
  background: transparent;
  color: ${({ theme }) => theme.colors.text};
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;

  &:hover:not(:disabled) {
    border-color: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 50%, ${({ theme }) => theme.colors.border});
    color: ${({ theme }) => theme.colors.accent};
  }
`;

export const FetchError = styled.p`
  margin: 0.75rem 0 0;
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.danger};
`;

export const RelatedSection = styled.section`
  margin-top: 1.5rem;
  padding-top: 1.25rem;
  border-top: 1px solid ${({ theme }) => theme.colors.border};
`;

export const RelatedTitle = styled.h3`
  margin: 0 0 0.85rem;
  font-size: 0.95rem;
`;

export const RelatedState = styled.p`
  margin: 0;
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const RelatedList = styled.ul`
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
`;

export const RelatedItem = styled.li`
  padding: 0.75rem 0.85rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.sm};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.bg} 35%, ${({ theme }) => theme.colors.surface});
`;

export const RelatedMeta = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.45rem;
  margin-bottom: 0.35rem;
  font-size: 0.75rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const RelatedBadge = styled.span`
  padding: 0.08rem 0.4rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 15%, transparent);
  color: ${({ theme }) => theme.colors.accent};
  font-weight: 600;
`;

export const RelatedNewsTitle = styled.p`
  margin: 0;
  font-size: 0.88rem;
  line-height: 1.4;
`;

export const RelatedSource = styled.span`
  display: block;
  margin-top: 0.25rem;
  font-size: 0.75rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;
