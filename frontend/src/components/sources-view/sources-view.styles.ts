import styled from 'styled-components';
import { mediaUp } from '../../styles/media';

export const Root = styled.section`
  display: flex;
  flex-direction: column;
  gap: 1rem;
`;

export const Toolbar = styled.div`
  display: flex;
  justify-content: stretch;

  ${mediaUp('sm')} {
    justify-content: flex-end;
  }
`;

export const AddButton = styled.button`
  width: 100%;
  min-height: 2.85rem;
  padding: 0.65rem 1.1rem;
  border: 1px solid ${({ theme }) => theme.colors.accent};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.accentMuted};
  color: ${({ theme }) => theme.colors.accent};
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: background ${({ theme }) => theme.transitions.fast};

  ${mediaUp('sm')} {
    width: auto;
  }

  &:hover {
    background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 24%, transparent);
  }

  &:active {
    transform: scale(0.99);
  }
`;
