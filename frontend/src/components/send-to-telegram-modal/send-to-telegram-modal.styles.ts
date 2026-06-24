import styled from 'styled-components';

export const Overlay = styled.div`
  position: fixed;
  inset: 0;
  z-index: 1100;
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
    width: min(480px, 100%);
    border-radius: ${({ theme }) => theme.radii.lg};
    padding-bottom: 0;
  }
`;

export const Header = styled.header`
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 1rem 1.1rem 0.5rem;
`;

export const Title = styled.h2`
  margin: 0;
  font-size: 1.05rem;
`;

export const CloseButton = styled.button`
  border: none;
  background: transparent;
  color: ${({ theme }) => theme.colors.textMuted};
  font-size: 1.5rem;
  line-height: 1;
  cursor: pointer;
`;

export const Body = styled.div`
  padding: 0.5rem 1.1rem 1rem;
  overflow-y: auto;
`;

export const Hint = styled.p`
  margin: 0 0 0.75rem;
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.textMuted};
  line-height: 1.45;
`;

export const ChannelList = styled.ul`
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
`;

export const ChannelButton = styled.button`
  width: 100%;
  text-align: left;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.surface};
  padding: 0.75rem 0.85rem;
  cursor: pointer;
  color: inherit;

  &:hover {
    border-color: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 45%, ${({ theme }) => theme.colors.border});
  }

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
`;

export const ChannelName = styled.div`
  font-weight: 600;
  margin-bottom: 0.2rem;
`;

export const ChannelMeta = styled.div`
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Error = styled.p`
  margin: 0.5rem 0 0;
  color: ${({ theme }) => theme.colors.danger};
  font-size: 0.85rem;
`;

export const Success = styled.p`
  margin: 0.5rem 0 0;
  color: ${({ theme }) => theme.colors.accent};
  font-size: 0.85rem;
`;
