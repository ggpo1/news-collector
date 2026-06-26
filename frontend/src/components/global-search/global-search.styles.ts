import styled from 'styled-components';

export const Root = styled.div`
  position: relative;
  width: 100%;
  max-width: 22rem;
`;

export const Input = styled.input`
  width: 100%;
  min-height: 2.5rem;
  padding: 0.55rem 0.85rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.bg};
  color: ${({ theme }) => theme.colors.text};
  font-size: 0.9rem;

  &:focus {
    outline: 2px solid color-mix(in srgb, ${({ theme }) => theme.colors.accent} 45%, transparent);
    outline-offset: 1px;
  }
`;

export const Dropdown = styled.div`
  position: absolute;
  top: calc(100% + 0.35rem);
  left: 0;
  right: 0;
  z-index: 30;
  max-height: 24rem;
  overflow-y: auto;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.bgElevated};
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.18);
`;

export const ResultButton = styled.button`
  display: block;
  width: 100%;
  padding: 0.75rem 0.9rem;
  border: none;
  border-bottom: 1px solid ${({ theme }) => theme.colors.borderSubtle};
  background: transparent;
  color: inherit;
  text-align: left;
  cursor: pointer;

  &:hover {
    background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 8%, transparent);
  }

  &:last-child {
    border-bottom: none;
  }
`;

export const ResultTitle = styled.div`
  font-size: 0.9rem;
  font-weight: 600;
  line-height: 1.35;
`;

export const ResultMeta = styled.div`
  margin-top: 0.25rem;
  font-size: 0.75rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const ResultSnippet = styled.div`
  margin-top: 0.35rem;
  font-size: 0.8rem;
  line-height: 1.4;
  color: ${({ theme }) => theme.colors.textMuted};

  b {
    color: ${({ theme }) => theme.colors.text};
    font-weight: 600;
  }
`;

export const Status = styled.div`
  padding: 0.85rem 0.9rem;
  font-size: 0.82rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;
