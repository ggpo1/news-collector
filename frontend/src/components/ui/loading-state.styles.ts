import styled from 'styled-components';

export const Root = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.85rem;
  min-height: 10rem;
  padding: 2rem 1rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Spinner = styled.div`
  width: 2rem;
  height: 2rem;
  border: 2px solid color-mix(in srgb, ${({ theme }) => theme.colors.accent} 20%, transparent);
  border-top-color: ${({ theme }) => theme.colors.accent};
  border-radius: 50%;
  animation: ui-spin 0.75s linear infinite;

  @keyframes ui-spin {
    to {
      transform: rotate(360deg);
    }
  }
`;

export const Label = styled.span`
  font-size: 0.9rem;
`;
