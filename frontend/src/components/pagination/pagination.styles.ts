import styled from 'styled-components';
import { mediaDown, mediaUp } from '../../styles/media';

export const Root = styled.nav`
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid ${({ theme }) => theme.colors.borderSubtle};
`;

export const Info = styled.span`
  font-size: 0.82rem;
  color: ${({ theme }) => theme.colors.textMuted};

  ${mediaDown('sm')} {
    width: 100%;
    text-align: center;
  }
`;

export const Buttons = styled.div`
  display: flex;
  gap: 0.5rem;
  width: 100%;

  ${mediaUp('sm')} {
    width: auto;
  }
`;

export const Button = styled.button`
  flex: 1;
  min-height: 2.75rem;
  padding: 0.5rem 0.9rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surface};
  color: ${({ theme }) => theme.colors.text};
  font-weight: 600;
  cursor: pointer;
  transition: background ${({ theme }) => theme.transitions.fast};

  ${mediaUp('sm')} {
    flex: initial;
    min-width: 6.5rem;
  }

  &:hover:not(:disabled) {
    background: ${({ theme }) => theme.colors.surfaceHover};
  }

  &:disabled {
    opacity: 0.45;
    cursor: not-allowed;
  }
`;
