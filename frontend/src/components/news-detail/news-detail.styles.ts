import styled from 'styled-components';
import { mediaUp } from '../../styles/media';

export const Panel = styled.aside`
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.lg};
  background: ${({ theme }) => theme.colors.surface};
  padding: 1rem;
  min-height: 280px;
  box-shadow: ${({ theme }) => theme.shadows.sm};

  ${mediaUp('md')} {
    padding: 1.25rem;
  }

  ${mediaUp('lg')} {
    min-height: 320px;
  }
`;

export const Placeholder = styled.div`
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 240px;
  color: ${({ theme }) => theme.colors.textMuted};
  text-align: center;
  padding: 1rem;
  font-size: 0.95rem;
  line-height: 1.5;
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
  font-size: 1.15rem;
  line-height: 1.4;
  letter-spacing: -0.02em;

  ${mediaUp('md')} {
    font-size: 1.25rem;
  }
`;

export const Content = styled.div`
  font-size: 0.95rem;
  line-height: 1.7;
  color: ${({ theme }) => theme.colors.text};
  white-space: pre-wrap;
`;

export const ExternalLink = styled.a`
  display: inline-flex;
  align-items: center;
  min-height: 2.75rem;
  font-size: 0.9rem;
  font-weight: 600;
  color: ${({ theme }) => theme.colors.accent};
  text-decoration: none;

  &:hover {
    text-decoration: underline;
  }
`;

export const State = styled.div<{ $error?: boolean }>`
  padding: 1.5rem 1rem;
  text-align: center;
  color: ${({ theme, $error }) => ($error ? theme.colors.danger : theme.colors.textMuted)};
`;

export const Actions = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
  margin-top: 1.25rem;

  ${mediaUp('sm')} {
    flex-direction: row;
    flex-wrap: wrap;
    align-items: center;
    gap: 0.75rem;
  }
`;

export const FetchButton = styled.button`
  min-height: 2.75rem;
  padding: 0.55rem 1rem;
  border-radius: ${({ theme }) => theme.radii.md};
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  border: 1px solid ${({ theme }) => theme.colors.accent};
  background: ${({ theme }) => theme.colors.accentMuted};
  color: ${({ theme }) => theme.colors.accent};
  transition: background ${({ theme }) => theme.transitions.fast};

  &:hover:not(:disabled) {
    background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 24%, transparent);
  }

  &:disabled {
    opacity: 0.6;
    cursor: wait;
  }
`;

export const RewriteButton = styled.button`
  min-height: 2.75rem;
  padding: 0.55rem 1rem;
  border-radius: ${({ theme }) => theme.radii.md};
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  border: 1px solid ${({ theme }) => theme.colors.border};
  background: transparent;
  color: ${({ theme }) => theme.colors.text};
  transition:
    background ${({ theme }) => theme.transitions.fast},
    border-color ${({ theme }) => theme.transitions.fast},
    color ${({ theme }) => theme.transitions.fast};

  &:hover:not(:disabled) {
    border-color: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 50%, ${({ theme }) => theme.colors.border});
    color: ${({ theme }) => theme.colors.accent};
  }

  &:disabled {
    opacity: 0.6;
    cursor: wait;
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
  border-top: 1px solid ${({ theme }) => theme.colors.borderSubtle};
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
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surfaceMuted};
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
  background: ${({ theme }) => theme.colors.accentMuted};
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
