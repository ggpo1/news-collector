import styled from 'styled-components';

export const Panel = styled.aside`
  padding: 1rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.lg};
  background: ${({ theme }) => theme.colors.surface};
  box-shadow: ${({ theme }) => theme.shadows.sm};

  @media (min-width: ${({ theme }) => theme.breakpoints.md}) {
    padding: 1.25rem;
  }
`;

export const Placeholder = styled.p`
  margin: 0;
  text-align: center;
  color: ${({ theme }) => theme.colors.textMuted};
  padding: 2rem 0.5rem;
`;

export const Header = styled.header`
  margin-bottom: 1rem;
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
  font-size: 1.15rem;
  line-height: 1.35;
`;

export const SourceBlock = styled.div`
  margin-top: 0.75rem;
  padding: 0.75rem 0.85rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.sm};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.bg} 35%, ${({ theme }) => theme.colors.surface});
`;

export const SourceLabel = styled.span`
  display: block;
  margin-bottom: 0.25rem;
  font-size: 0.75rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const SourceTitle = styled.p`
  margin: 0;
  font-size: 0.88rem;
  line-height: 1.4;
`;

export const Summary = styled.p`
  margin: 1rem 0 0;
  font-size: 0.92rem;
  line-height: 1.55;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Content = styled.div`
  margin-top: 1rem;
  font-size: 0.95rem;
  line-height: 1.65;
  white-space: pre-wrap;
`;

export const Actions = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.6rem;
  margin-top: 1.25rem;
  padding-top: 1rem;
  border-top: 1px solid ${({ theme }) => theme.colors.border};
`;

export const PrimaryButton = styled.button`
  padding: 0.5rem 1rem;
  border: 1px solid ${({ theme }) => theme.colors.accent};
  border-radius: ${({ theme }) => theme.radii.sm};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 18%, transparent);
  color: ${({ theme }) => theme.colors.accent};
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;

  &:hover:not(:disabled) {
    background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 28%, transparent);
  }

  &:disabled {
    opacity: 0.6;
    cursor: wait;
  }
`;

export const TelegramButton = styled.button`
  padding: 0.5rem 1rem;
  border: 1px solid color-mix(in srgb, #2aabee 55%, ${({ theme }) => theme.colors.border});
  border-radius: ${({ theme }) => theme.radii.sm};
  background: transparent;
  color: #2aabee;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;

  &:hover:not(:disabled) {
    background: color-mix(in srgb, #2aabee 12%, transparent);
  }
`;

export const DangerButton = styled.button`
  padding: 0.5rem 1rem;
  border: 1px solid color-mix(in srgb, ${({ theme }) => theme.colors.danger} 55%, ${({ theme }) => theme.colors.border});
  border-radius: ${({ theme }) => theme.radii.sm};
  background: transparent;
  color: ${({ theme }) => theme.colors.danger};
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;

  &:hover:not(:disabled) {
    background: color-mix(in srgb, ${({ theme }) => theme.colors.danger} 12%, transparent);
  }

  &:disabled {
    opacity: 0.6;
    cursor: wait;
  }
`;

export const Error = styled.p`
  margin: 0.75rem 0 0;
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.danger};
`;
