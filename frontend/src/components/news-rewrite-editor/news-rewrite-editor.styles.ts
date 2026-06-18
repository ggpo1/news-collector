import styled from 'styled-components';

export const Overlay = styled.div`
  position: fixed;
  inset: 0;
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1.5rem;
  background: color-mix(in srgb, ${({ theme }) => theme.colors.bg} 55%, transparent);
  backdrop-filter: blur(4px);
`;

export const Dialog = styled.div`
  position: relative;
  width: min(720px, 100%);
  max-height: min(90vh, 900px);
  display: flex;
  flex-direction: column;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surface};
  box-shadow: 0 24px 48px color-mix(in srgb, ${({ theme }) => theme.colors.bg} 70%, transparent);
`;

export const Header = styled.header`
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  padding: 1.25rem 1.25rem 0.75rem;
  border-bottom: 1px solid ${({ theme }) => theme.colors.border};
`;

export const HeaderText = styled.div`
  min-width: 0;
`;

export const Title = styled.h2`
  margin: 0;
  font-size: 1.15rem;
  line-height: 1.35;
`;

export const Subtitle = styled.p`
  margin: 0.35rem 0 0;
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.textMuted};
  line-height: 1.4;
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

export const TextArea = styled.textarea`
  min-height: 5rem;
  padding: 0.65rem 0.75rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.sm};
  background: ${({ theme }) => theme.colors.bg};
  color: ${({ theme }) => theme.colors.text};
  font: inherit;
  font-weight: 400;
  line-height: 1.55;
  resize: vertical;

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

export const ContentArea = styled(TextArea)`
  min-height: 220px;
`;

export const Footer = styled.footer`
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  padding: 1rem 1.25rem;
  border-top: 1px solid ${({ theme }) => theme.colors.border};
`;

export const FooterGroup = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.6rem;
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

export const AiButton = styled(SecondaryButton)`
  border-color: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 45%, ${({ theme }) => theme.colors.border});
  color: ${({ theme }) => theme.colors.accent};
`;

export const State = styled.p`
  margin: 0;
  padding: 1rem 0;
  text-align: center;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Error = styled.p`
  margin: 0;
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.danger};
`;

export const Success = styled.p`
  margin: 0;
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.accent};
`;

export const AiLoaderOverlay = styled.div`
  position: absolute;
  inset: 0;
  z-index: 2;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  padding: 2rem;
  border-radius: inherit;
  background: color-mix(in srgb, ${({ theme }) => theme.colors.surface} 88%, transparent);
  backdrop-filter: blur(2px);
`;

export const Spinner = styled.div`
  width: 2.5rem;
  height: 2.5rem;
  border: 3px solid color-mix(in srgb, ${({ theme }) => theme.colors.accent} 20%, transparent);
  border-top-color: ${({ theme }) => theme.colors.accent};
  border-radius: 50%;
  animation: ai-spin 0.8s linear infinite;

  @keyframes ai-spin {
    to {
      transform: rotate(360deg);
    }
  }
`;

export const LoaderTitle = styled.p`
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
  color: ${({ theme }) => theme.colors.text};
  text-align: center;
`;

export const LoaderHint = styled.p`
  margin: 0;
  max-width: 22rem;
  font-size: 0.85rem;
  line-height: 1.5;
  color: ${({ theme }) => theme.colors.textMuted};
  text-align: center;
`;
