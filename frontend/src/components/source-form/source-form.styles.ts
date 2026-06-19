import styled from 'styled-components';

export const Overlay = styled.div`
  position: fixed;
  inset: 0;
  z-index: 1000;
  display: flex;
  align-items: flex-end;
  justify-content: center;
  padding: 0;
  background: color-mix(in srgb, ${({ theme }) => theme.colors.bg} 55%, transparent);
  backdrop-filter: blur(4px);

  @media (min-width: ${({ theme }) => theme.breakpoints.md}) {
    align-items: center;
    padding: 1.5rem;
  }
`;

export const Dialog = styled.div`
  width: 100%;
  max-height: 92dvh;
  display: flex;
  flex-direction: column;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.lg} ${({ theme }) => theme.radii.lg} 0 0;
  background: ${({ theme }) => theme.colors.surface};
  box-shadow: ${({ theme }) => theme.shadows.lg};
  padding-bottom: env(safe-area-inset-bottom, 0px);

  @media (min-width: ${({ theme }) => theme.breakpoints.md}) {
    width: min(560px, 100%);
    max-height: min(90vh, 800px);
    border-radius: ${({ theme }) => theme.radii.lg};
    padding-bottom: 0;
  }
`;

export const Header = styled.header`
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  padding: 1.25rem 1.25rem 0.75rem;
  border-bottom: 1px solid ${({ theme }) => theme.colors.border};
`;

export const Title = styled.h2`
  margin: 0;
  font-size: 1.15rem;
  line-height: 1.35;
`;

export const CloseButton = styled.button`
  flex-shrink: 0;
  border: none;
  width: 2rem;
  height: 2rem;
  border-radius: ${({ theme }) => theme.radii.sm};
  background: transparent;
  color: ${({ theme }) => theme.colors.textMuted};
  font-size: 1.25rem;
  line-height: 1;
  cursor: pointer;

  &:hover:not(:disabled) {
    background: ${({ theme }) => theme.colors.surfaceHover};
    color: ${({ theme }) => theme.colors.text};
  }

  &:disabled {
    opacity: 0.4;
    cursor: not-allowed;
  }
`;

export const Body = styled.div`
  flex: 1;
  overflow-y: auto;
  padding: 1.25rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
`;

export const Field = styled.label`
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  font-size: 0.82rem;
  font-weight: 600;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Input = styled.input`
  padding: 0.65rem 0.75rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.sm};
  background: ${({ theme }) => theme.colors.bg};
  color: ${({ theme }) => theme.colors.text};
  font: inherit;
  font-weight: 400;

  &:focus {
    outline: none;
    border-color: ${({ theme }) => theme.colors.accent};
    box-shadow: 0 0 0 1px ${({ theme }) => theme.colors.accent};
  }

  &:disabled {
    opacity: 0.65;
    cursor: not-allowed;
  }
`;

export const Select = styled.select`
  padding: 0.65rem 0.75rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.sm};
  background: ${({ theme }) => theme.colors.bg};
  color: ${({ theme }) => theme.colors.text};
  font: inherit;
  font-weight: 400;

  &:focus {
    outline: none;
    border-color: ${({ theme }) => theme.colors.accent};
    box-shadow: 0 0 0 1px ${({ theme }) => theme.colors.accent};
  }

  &:disabled {
    opacity: 0.65;
    cursor: not-allowed;
  }
`;

export const CheckboxRow = styled.label`
  display: flex;
  align-items: center;
  gap: 0.55rem;
  font-size: 0.9rem;
  font-weight: 500;
  color: ${({ theme }) => theme.colors.text};
  cursor: pointer;

  input {
    width: 1rem;
    height: 1rem;
    accent-color: ${({ theme }) => theme.colors.accent};
  }
`;

export const Footer = styled.footer`
  display: flex;
  justify-content: flex-end;
  gap: 0.6rem;
  padding: 1rem 1.25rem;
  border-top: 1px solid ${({ theme }) => theme.colors.border};
`;

export const PrimaryButton = styled.button`
  padding: 0.5rem 1rem;
  border: 1px solid ${({ theme }) => theme.colors.accent};
  border-radius: ${({ theme }) => theme.radii.sm};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 18%, transparent);
  color: ${({ theme }) => theme.colors.accent};
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;

  &:hover:not(:disabled) {
    background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 28%, transparent);
  }

  &:disabled {
    opacity: 0.6;
    cursor: wait;
  }
`;

export const SecondaryButton = styled.button`
  padding: 0.5rem 1rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.sm};
  background: transparent;
  color: ${({ theme }) => theme.colors.text};
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;

  &:hover:not(:disabled) {
    background: ${({ theme }) => theme.colors.surfaceHover};
  }

  &:disabled {
    opacity: 0.6;
    cursor: wait;
  }
`;

export const Error = styled.p`
  margin: 0;
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.danger};
`;
