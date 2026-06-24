import styled, { css } from 'styled-components';
import { mediaUp } from '../../styles/media';

export const List = styled.ul`
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.65rem;

  ${mediaUp('md')} {
    gap: 0.75rem;
  }
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
`;

export const Meta = styled.div`
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.5rem;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const SourceName = styled.span`
  font-weight: 600;
  color: ${({ theme }) => theme.colors.text};
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

export const Badge = styled.span`
  padding: 0.12rem 0.5rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  background: ${({ theme }) => theme.colors.accentMuted};
  color: ${({ theme }) => theme.colors.accent};
  font-size: 0.7rem;
  font-weight: 600;
`;

export const CategoryBadge = styled.span<{ $muted?: boolean }>`
  padding: 0.12rem 0.5rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  border: 1px solid ${({ theme }) => theme.colors.border};
  background: ${({ theme, $muted }) =>
    $muted ? theme.colors.surface : theme.colors.surfaceMuted};
  color: ${({ theme, $muted }) => ($muted ? theme.colors.textFaint : theme.colors.textMuted)};
  font-size: 0.7rem;
  font-weight: 600;
`;

function toneColor(value: number | null | undefined, theme: { colors: { danger: string; textMuted: string; accent: string } }) {
  if (value === null || value === undefined) {
    return theme.colors.textMuted;
  }

  if (value <= -0.2) {
    return theme.colors.danger;
  }

  if (value >= 0.2) {
    return theme.colors.accent;
  }

  return theme.colors.textMuted;
}

export const ToneBadge = styled.span<{ $value?: number | null; $muted?: boolean }>`
  padding: 0.12rem 0.5rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  border: 1px solid ${({ theme }) => theme.colors.border};
  background: ${({ theme }) => theme.colors.surface};
  color: ${({ theme, $value, $muted }) =>
    $muted ? theme.colors.textFaint : toneColor($value, theme)};
  font-size: 0.7rem;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
`;

export const Title = styled.h2`
  margin: 0.45rem 0 0;
  font-size: 1rem;
  line-height: 1.45;
  color: ${({ theme }) => theme.colors.text};
`;

export const Summary = styled.p`
  margin: 0.45rem 0 0;
  font-size: 0.88rem;
  line-height: 1.55;
  color: ${({ theme }) => theme.colors.textMuted};
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
`;
