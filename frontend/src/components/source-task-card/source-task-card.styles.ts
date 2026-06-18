import styled from 'styled-components';

export const Card = styled.div<{ $compact?: boolean }>`
  padding: ${({ $compact }) => ($compact ? '0.75rem 0.85rem' : '0.9rem 1rem')};
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.sm};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.bg} 35%, ${({ theme }) => theme.colors.surface});
`;

export const Label = styled.span`
  display: block;
  margin-bottom: 0.4rem;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Meta = styled.div`
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.45rem;
  margin-bottom: 0.35rem;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Badge = styled.span`
  padding: 0.08rem 0.4rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 12%, transparent);
  color: ${({ theme }) => theme.colors.accent};
  font-weight: 600;
`;

export const Title = styled.p`
  margin: 0;
  font-size: 0.9rem;
  line-height: 1.45;
  color: ${({ theme }) => theme.colors.text};
`;

export const Actions = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.65rem;
  margin-top: 0.65rem;
`;

export const LinkButton = styled.button`
  border: none;
  padding: 0;
  background: none;
  color: ${({ theme }) => theme.colors.accent};
  font-size: 0.82rem;
  font-weight: 600;
  cursor: pointer;

  &:hover {
    text-decoration: underline;
  }
`;

export const ExternalLink = styled.a`
  font-size: 0.82rem;
  color: ${({ theme }) => theme.colors.textMuted};
  text-decoration: none;

  &:hover {
    color: ${({ theme }) => theme.colors.accent};
    text-decoration: underline;
  }
`;
