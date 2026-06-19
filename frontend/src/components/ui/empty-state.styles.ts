import styled from 'styled-components';

export const Root = styled.div<{ $error?: boolean }>`
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  min-height: 12rem;
  padding: 2rem 1.25rem;
  text-align: center;
  border: 1px dashed
    ${({ theme, $error }) =>
      $error
        ? `color-mix(in srgb, ${theme.colors.danger} 40%, transparent)`
        : theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.lg};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.surface} 55%, transparent);
  color: ${({ theme, $error }) => ($error ? theme.colors.danger : theme.colors.textMuted)};
  font-size: 0.95rem;
  line-height: 1.5;
`;
