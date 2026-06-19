import styled from 'styled-components';

export const Toolbar = styled.div`
  margin-bottom: 1rem;
`;

export const ErrorBanner = styled.p`
  margin: 0 0 1rem;
  padding: 0.75rem 0.9rem;
  border: 1px solid color-mix(in srgb, ${({ theme }) => theme.colors.danger} 35%, transparent);
  border-radius: ${({ theme }) => theme.radii.md};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.danger} 10%, transparent);
  color: ${({ theme }) => theme.colors.danger};
  font-size: 0.88rem;
`;
