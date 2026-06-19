import styled, { css } from 'styled-components';
import { mediaUp } from '../../styles/media';

export const List = styled.ul`
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 0.75rem;

  ${mediaUp('md')} {
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 1rem;
  }

  ${mediaUp('xl')} {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
`;

export const Card = styled.article`
  display: flex;
  flex-direction: column;
  height: 100%;
  padding: 1rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.lg};
  background: ${({ theme }) => theme.colors.surface};
  box-shadow: ${({ theme }) => theme.shadows.sm};
`;

export const CardHeader = styled.div`
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 0.75rem;
`;

export const Title = styled.h2`
  margin: 0;
  font-size: 1rem;
  line-height: 1.4;
`;

export const Meta = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.45rem;
  margin-top: 0.45rem;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Badge = styled.span<{ $muted?: boolean }>`
  flex-shrink: 0;
  padding: 0.12rem 0.5rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  font-size: 0.72rem;
  font-weight: 600;

  ${({ theme, $muted }) =>
    $muted
      ? css`
          background: color-mix(in srgb, ${theme.colors.textMuted} 15%, transparent);
          color: ${theme.colors.textMuted};
        `
      : css`
          background: ${theme.colors.accentMuted};
          color: ${theme.colors.accent};
        `}
`;

export const Url = styled.a`
  display: block;
  margin-top: 0.75rem;
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.accent};
  word-break: break-all;
  text-decoration: none;

  &:hover {
    text-decoration: underline;
  }
`;

export const Actions = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: auto;
  padding-top: 1rem;
`;

export const ActionButton = styled.button`
  flex: 1;
  min-height: 2.65rem;
  padding: 0.45rem 0.75rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: transparent;
  color: ${({ theme }) => theme.colors.text};
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;

  ${mediaUp('sm')} {
    flex: initial;
  }

  &:hover:not(:disabled) {
    background: ${({ theme }) => theme.colors.surfaceHover};
  }

  &:disabled {
    opacity: 0.6;
    cursor: wait;
  }
`;

export const DangerButton = styled(ActionButton)`
  border-color: color-mix(in srgb, ${({ theme }) => theme.colors.danger} 40%, transparent);
  color: ${({ theme }) => theme.colors.danger};
`;
