import styled from 'styled-components';

export const FiltersRow = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
`;

export const FilterLabel = styled.span`
  display: block;
  margin-bottom: 0.25rem;
  font-size: 0.82rem;
  font-weight: 600;
`;

export const Select = styled.select`
  padding: 0.45rem 0.65rem;
  border-radius: ${({ theme }) => theme.radii.sm};
  border: 1px solid ${({ theme }) => theme.colors.border};
  background: ${({ theme }) => theme.colors.surface};
  font: inherit;
  min-width: 180px;
`;
