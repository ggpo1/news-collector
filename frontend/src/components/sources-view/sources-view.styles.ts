import styled from 'styled-components';

export const Root = styled.section`
  display: flex;
  flex-direction: column;
  gap: 1rem;
`;

export const Toolbar = styled.div`
  display: flex;
  justify-content: flex-end;
`;

export const AddButton = styled.button`
  padding: 0.55rem 1rem;
  border: 1px solid ${({ theme }) => theme.colors.accent};
  border-radius: ${({ theme }) => theme.radii.sm};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 18%, transparent);
  color: ${({ theme }) => theme.colors.accent};
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;

  &:hover {
    background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 28%, transparent);
  }
`;
