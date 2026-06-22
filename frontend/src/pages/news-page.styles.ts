import styled from 'styled-components';
import { mediaUp } from '../../styles/media';

export const FiltersRow = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.75rem;

  ${mediaUp('sm')} {
    flex-direction: row;
    flex-wrap: wrap;
    align-items: flex-end;
    gap: 1rem;
  }
`;

export const ErrorBanner = styled.p`
  margin: 0;
  padding: 0.75rem 0.9rem;
  border: 1px solid color-mix(in srgb, ${({ theme }) => theme.colors.danger} 35%, transparent);
  border-radius: ${({ theme }) => theme.radii.md};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.danger} 10%, transparent);
  color: ${({ theme }) => theme.colors.danger};
  font-size: 0.88rem;
`;
