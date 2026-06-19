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

  ${mediaUp('sm')} {
    width: auto;
  }
`;

export const List = styled.ul`
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
`;

export const Card = styled.li`
  padding: 1rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.lg};
  background: ${({ theme }) => theme.colors.surface};
`;

export const CardHeader = styled.div`
  display: flex;
  justify-content: space-between;
  gap: 0.75rem;
`;

export const Name = styled.h2`
  margin: 0;
  font-size: 1rem;
`;

export const Login = styled.p`
  margin: 0.25rem 0 0;
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Badge = styled.span<{ $muted?: boolean }>`
  flex-shrink: 0;
  align-self: flex-start;
  padding: 0.12rem 0.5rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  font-size: 0.72rem;
  font-weight: 600;
  background: ${({ theme, $muted }) =>
    $muted ? 'color-mix(in srgb, ' + theme.colors.textMuted + ' 15%, transparent)' : theme.colors.accentMuted};
  color: ${({ theme, $muted }) => ($muted ? theme.colors.textMuted : theme.colors.accent)};
`;

export const Overlay = styled.div`
  position: fixed;
  inset: 0;
  z-index: 1000;
  display: flex;
  align-items: flex-end;
  justify-content: center;
  background: color-mix(in srgb, ${({ theme }) => theme.colors.bg} 55%, transparent);
  backdrop-filter: blur(4px);

  ${mediaUp('md')} {
    align-items: center;
    padding: 1.5rem;
  }
`;

export const Dialog = styled.div`
  width: 100%;
  max-height: 92dvh;
  overflow-y: auto;
  padding: 1.25rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.lg} ${({ theme }) => theme.radii.lg} 0 0;
  background: ${({ theme }) => theme.colors.surface};

  ${mediaUp('md')} {
    width: min(480px, 100%);
    border-radius: ${({ theme }) => theme.radii.lg};
  }
`;

export const Title = styled.h2`
  margin: 0 0 1rem;
  font-size: 1.1rem;
`;

export const Field = styled.label`
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  margin-bottom: 0.85rem;
  font-size: 0.82rem;
  font-weight: 600;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Input = styled.input`
  min-height: 2.75rem;
  padding: 0.6rem 0.8rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.bg};
  color: ${({ theme }) => theme.colors.text};
`;

export const Select = styled.select`
  min-height: 2.75rem;
  padding: 0.6rem 0.8rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.bg};
  color: ${({ theme }) => theme.colors.text};
`;

export const Actions = styled.div`
  display: flex;
  justify-content: flex-end;
  gap: 0.6rem;
  margin-top: 1rem;
`;

export const Button = styled.button`
  min-height: 2.65rem;
  padding: 0.5rem 1rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: transparent;
  cursor: pointer;
`;

export const PrimaryButton = styled(Button)`
  border-color: ${({ theme }) => theme.colors.accent};
  background: ${({ theme }) => theme.colors.accentMuted};
  color: ${({ theme }) => theme.colors.accent};
  font-weight: 600;
`;

export const Error = styled.p`
  margin: 0 0 0.75rem;
  color: ${({ theme }) => theme.colors.danger};
  font-size: 0.85rem;
`;
