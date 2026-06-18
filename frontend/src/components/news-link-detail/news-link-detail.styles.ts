import styled from 'styled-components';

export const Panel = styled.aside`
  position: sticky;
  top: 1.5rem;
  padding: 1.25rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surface};
`;

export const Placeholder = styled.p`
  margin: 0;
  text-align: center;
  color: ${({ theme }) => theme.colors.textMuted};
  padding: 2rem 0.5rem;
`;

export const State = styled.p<{ $error?: boolean }>`
  margin: 0;
  text-align: center;
  color: ${({ theme, $error }) => ($error ? theme.colors.danger : theme.colors.textMuted)};
`;

export const Header = styled.div`
  margin-bottom: 1.25rem;
`;

export const Meta = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
  font-size: 0.8rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Badge = styled.span`
  padding: 0.15rem 0.55rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 15%, transparent);
  color: ${({ theme }) => theme.colors.accent};
  font-weight: 600;
`;

export const Title = styled.h2`
  margin: 0;
  font-size: 1.05rem;
  line-height: 1.35;
`;

export const ConfidenceBar = styled.div`
  margin: 1rem 0 1.25rem;
`;

export const ConfidenceLabel = styled.div`
  display: flex;
  justify-content: space-between;
  margin-bottom: 0.35rem;
  font-size: 0.8rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const ConfidenceTrack = styled.div`
  height: 6px;
  border-radius: ${({ theme }) => theme.radii.pill};
  background: ${({ theme }) => theme.colors.border};
  overflow: hidden;
`;

export const ConfidenceFill = styled.div<{ $value: number }>`
  height: 100%;
  width: ${({ $value }) => `${Math.min(100, Math.max(0, $value * 100))}%`};
  background: ${({ theme }) => theme.colors.accent};
  border-radius: inherit;
`;

export const Articles = styled.div`
  display: flex;
  flex-direction: column;
  gap: 1rem;
`;

export const Article = styled.article`
  padding: 0.9rem 1rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.sm};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.bg} 40%, ${({ theme }) => theme.colors.surface});
`;

export const ArticleMeta = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-bottom: 0.45rem;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const ArticleTitle = styled.h3`
  margin: 0 0 0.5rem;
  font-size: 0.95rem;
  line-height: 1.4;
`;

export const ArticleSummary = styled.p`
  margin: 0 0 0.65rem;
  font-size: 0.85rem;
  line-height: 1.5;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const ExternalLink = styled.a`
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.accent};
  text-decoration: none;

  &:hover {
    text-decoration: underline;
  }
`;
