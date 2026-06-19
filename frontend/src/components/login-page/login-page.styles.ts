import styled from 'styled-components';

export const Page = styled.div`
  min-height: 100dvh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1.5rem;
`;

export const Card = styled.div`
  width: min(420px, 100%);
  padding: 1.5rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.lg};
  background: ${({ theme }) => theme.colors.surface};
  box-shadow: ${({ theme }) => theme.shadows.lg};
`;

export const Title = styled.h1`
  margin: 0;
  font-size: 1.45rem;
  letter-spacing: -0.03em;
`;

export const Subtitle = styled.p`
  margin: 0.4rem 0 1.25rem;
  color: ${({ theme }) => theme.colors.textMuted};
  font-size: 0.92rem;
  line-height: 1.5;
`;

export const Form = styled.form`
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
  min-height: 2.85rem;
  padding: 0.65rem 0.85rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.bg};
  color: ${({ theme }) => theme.colors.text};
`;

export const Button = styled.button`
  min-height: 2.85rem;
  margin-top: 0.25rem;
  border: 1px solid ${({ theme }) => theme.colors.accent};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.accentMuted};
  color: ${({ theme }) => theme.colors.accent};
  font-size: 0.95rem;
  font-weight: 700;
  cursor: pointer;

  &:disabled {
    opacity: 0.65;
    cursor: wait;
  }
`;

export const Error = styled.p`
  margin: 0;
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.danger};
`;

export const Hint = styled.p`
  margin: 1rem 0 0;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textFaint};
  line-height: 1.45;
  justify-content: center;
`;

export const Footer = styled.div`
  margin-top: 1rem;
  display: flex;
  justify-content: center;
  flex-direction: column;
  gap: 1rem;
`;

export const LinkButton = styled.button`
  border: none;
  background: none;
  padding: 0;
  color: ${({ theme }) => theme.colors.accent};
  font-size: 0.88rem;
  font-weight: 600;
  cursor: pointer;
  text-decoration: underline;
  text-underline-offset: 0.15em;

  &:disabled {
    opacity: 0.65;
    cursor: wait;
  }
`;
