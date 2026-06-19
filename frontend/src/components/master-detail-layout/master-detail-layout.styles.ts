import styled from 'styled-components';
import { mediaUp } from '../../styles/media';

export const DesktopGrid = styled.div`
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(300px, 400px);
  gap: 1.25rem;
  flex: 1;
  min-height: 0;
  align-items: stretch;

  ${mediaUp('xl')} {
    grid-template-columns: minmax(0, 1fr) minmax(320px, 440px);
    gap: 1.5rem;
  }
`;

export const ListColumn = styled.section<{ $sticky?: boolean }>`
  min-width: 0;

  ${({ $sticky }) =>
    $sticky &&
    `
    display: flex;
    flex-direction: column;
    flex: 1;
    min-height: 0;
  `}
`;

export const DetailColumn = styled.aside`
  min-width: 0;
  min-height: 0;
  overflow-y: auto;
  overscroll-behavior: contain;
  -webkit-overflow-scrolling: touch;
`;

export const MobileDetail = styled.section`
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
`;

export const BackBar = styled.button`
  display: flex;
  align-items: center;
  gap: 0.45rem;
  width: fit-content;
  margin-bottom: 0.85rem;
  padding: 0.45rem 0.65rem 0.45rem 0.35rem;
  border: none;
  border-radius: ${({ theme }) => theme.radii.pill};
  background: ${({ theme }) => theme.colors.surface};
  color: ${({ theme }) => theme.colors.accent};
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;

  &:active {
    transform: scale(0.98);
  }
`;

export const BackIcon = styled.span`
  font-size: 1.1rem;
  line-height: 1;
`;

export const MobileDetailBody = styled.div`
  flex: 1;
  min-height: 0;
  overflow-y: auto;
  -webkit-overflow-scrolling: touch;
`;
