import styled from 'styled-components';

export const Panel = styled.section`
  display: flex;
  flex-direction: column;
  gap: 0.9rem;
  padding: 1rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surfaceMuted};
`;

export const Title = styled.h3`
  margin: 0;
  font-size: 0.95rem;
`;

export const Hint = styled.p`
  margin: -0.35rem 0 0;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Block = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
`;

export const Label = styled.div`
  font-size: 0.82rem;
  font-weight: 600;
`;

export const TagList = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem 0.85rem;
`;

export const TagOption = styled.label`
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  font-size: 0.82rem;
  cursor: pointer;
`;

export const SaveButton = styled.button`
  align-self: flex-start;
  padding: 0.45rem 0.85rem;
  border-radius: ${({ theme }) => theme.radii.sm};
  border: 1px solid ${({ theme }) => theme.colors.border};
  background: ${({ theme }) => theme.colors.surface};
  cursor: pointer;
  font: inherit;

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
`;

export const Error = styled.p`
  margin: 0;
  color: ${({ theme }) => theme.colors.danger};
  font-size: 0.82rem;
`;
