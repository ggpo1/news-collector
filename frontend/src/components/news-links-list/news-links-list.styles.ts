import styled, { css } from 'styled-components';
import { mediaUp } from '../../styles/media';

export const List = styled.ul`
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
`;

export const State = styled.div<{ $error?: boolean }>`
  padding: 2rem 1rem;
  text-align: center;
  color: ${({ theme, $error }) => ($error ? theme.colors.danger : theme.colors.textMuted)};
  border: 1px dashed
    ${({ theme, $error }) =>
      $error ? `color-mix(in srgb, ${theme.colors.danger} 40%, transparent)` : theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
`;

export const Card = styled.button<{ $active?: boolean }>`
  width: 100%;
  text-align: left;
  padding: 0.95rem 1rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surface};
  color: inherit;
  cursor: pointer;
  transition:
    border-color ${({ theme }) => theme.transitions.fast},
    box-shadow ${({ theme }) => theme.transitions.fast},
    transform ${({ theme }) => theme.transitions.fast};

  &:hover {
    border-color: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 45%, ${({ theme }) => theme.colors.border});
    box-shadow: ${({ theme }) => theme.shadows.sm};
  }

  &:active {
    transform: scale(0.995);
  }

  ${({ theme, $active }) =>
    $active &&
    css`
      border-color: ${theme.colors.accent};
      box-shadow: 0 0 0 1px color-mix(in srgb, ${theme.colors.accent} 55%, transparent);
      background: color-mix(in srgb, ${theme.colors.accent} 6%, ${theme.colors.surface});
    `}
`;

export const CardTop = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  margin-bottom: 0.65rem;
`;

export const Meta = styled.div`
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Chevron = styled.span`
  flex-shrink: 0;
  font-size: 1.25rem;
  line-height: 1;
  color: ${({ theme }) => theme.colors.textFaint};

  ${mediaUp('lg')} {
    display: none;
  }
`;

export const Badge = styled.span<{ $variant?: 'topic' | 'duplicate' | 'related' }>`
  padding: 0.1rem 0.45rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  font-size: 0.72rem;
  font-weight: 600;
  background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 15%, transparent);
  color: ${({ theme }) => theme.colors.accent};

  ${({ $variant, theme }) =>
    $variant === 'duplicate' &&
    css`
      background: color-mix(in srgb, ${theme.colors.danger} 15%, transparent);
      color: ${theme.colors.danger};
    `}
`;

export const Pair = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.55rem;
`;

export const NewsLine = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
`;

export const Source = styled.span`
  font-size: 0.75rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Title = styled.span`
  font-size: 0.92rem;
  line-height: 1.4;
  color: ${({ theme }) => theme.colors.text};
`;

export const Connector = styled.span`
  align-self: center;
  font-size: 0.75rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;
