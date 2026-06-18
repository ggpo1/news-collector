import styled, { css } from 'styled-components';

export const List = styled.ul`
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;

  & > li {
    display: flex;
    flex-direction: column;
    gap: 0.45rem;
  }
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
  padding: 1rem 1.1rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surface};
  color: inherit;
  cursor: pointer;
  transition: border-color 0.15s, box-shadow 0.15s;

  &:hover {
    border-color: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 50%, ${({ theme }) => theme.colors.border});
  }

  ${({ theme, $active }) =>
    $active &&
    css`
      border-color: ${theme.colors.accent};
      box-shadow: 0 0 0 1px ${theme.colors.accent};
    `}
`;

export const Meta = styled.div`
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.45rem;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Badge = styled.span`
  padding: 0.1rem 0.45rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 15%, transparent);
  color: ${({ theme }) => theme.colors.accent};
  font-size: 0.72rem;
  font-weight: 600;
`;

export const Title = styled.h2`
  margin: 0;
  font-size: 1rem;
  line-height: 1.4;
  color: ${({ theme }) => theme.colors.text};
`;

export const Preview = styled.p`
  margin: 0.5rem 0 0;
  font-size: 0.88rem;
  line-height: 1.5;
  color: ${({ theme }) => theme.colors.textMuted};
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
`;

export const TaskWrap = styled.div`
  padding-left: 0.15rem;
`;
