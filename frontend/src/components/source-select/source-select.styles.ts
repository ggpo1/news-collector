import styled from 'styled-components';
import { mediaUp } from '../../styles/media';

export const Root = styled.label`
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  width: 100%;

  ${mediaUp('sm')} {
    width: auto;
    min-width: 16rem;
    max-width: 24rem;
  }

  ${mediaUp('lg')} {
    max-width: 20rem;
  }
`;

export const Label = styled.span`
  font-size: 0.78rem;
  font-weight: 600;
  color: ${({ theme }) => theme.colors.textMuted};
  text-transform: uppercase;
  letter-spacing: 0.05em;
`;

export const Select = styled.select`
  width: 100%;
  min-height: 2.75rem;
  padding: 0.65rem 0.85rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surface};
  color: ${({ theme }) => theme.colors.text};
  font-size: 0.95rem;
  box-shadow: ${({ theme }) => theme.shadows.sm};

  &:focus-visible {
    border-color: ${({ theme }) => theme.colors.accent};
  }

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
`;
