import styled, { css } from 'styled-components';

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

export const Card = styled.article`
  padding: 1rem 1.1rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surface};
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
  font-size: 0.8rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Badge = styled.span<{ $muted?: boolean }>`
  padding: 0.1rem 0.45rem;
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
          background: color-mix(in srgb, ${theme.colors.accent} 15%, transparent);
          color: ${theme.colors.accent};
        `}
`;

export const Url = styled.a`
  display: block;
  margin-top: 0.55rem;
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
  gap: 0.45rem;
  margin-top: 0.85rem;
`;

export const ActionButton = styled.button`
  padding: 0.35rem 0.7rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.sm};
  background: transparent;
  color: ${({ theme }) => theme.colors.text};
  font-size: 0.82rem;
  cursor: pointer;

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
