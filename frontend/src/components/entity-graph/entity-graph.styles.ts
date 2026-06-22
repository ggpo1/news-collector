import styled from 'styled-components';
import { mediaUp } from '../../styles/media';

export const GraphLayout = styled.div`
  display: flex;
  flex: 1;
  min-height: 0;
  gap: 0;
  position: relative;

  ${mediaUp('lg')} {
    gap: 1rem;
  }
`;

export const GraphMain = styled.div`
  display: flex;
  flex-direction: column;
  flex: 1;
  min-width: 0;
  min-height: 0;
`;

export const ControlsBar = styled.div`
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  border-bottom: 1px solid ${({ theme }) => theme.colors.borderSubtle};
  background: ${({ theme }) => theme.colors.bgElevated};
`;

export const ControlGroup = styled.label`
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  font-size: 0.72rem;
  font-weight: 600;
  color: ${({ theme }) => theme.colors.textMuted};
  text-transform: uppercase;
  letter-spacing: 0.04em;
`;

export const Select = styled.select`
  min-width: 7.5rem;
  padding: 0.45rem 0.65rem;
  border-radius: ${({ theme }) => theme.radii.sm};
  border: 1px solid ${({ theme }) => theme.colors.border};
  background: ${({ theme }) => theme.colors.surface};
  color: ${({ theme }) => theme.colors.text};
  font-size: 0.9rem;
`;

export const RangeInput = styled.input`
  width: 7rem;
  accent-color: ${({ theme }) => theme.colors.accent};
`;

export const RangeValue = styled.span`
  font-size: 0.85rem;
  color: ${({ theme }) => theme.colors.text};
  font-weight: 500;
  text-transform: none;
  letter-spacing: normal;
`;

export const RefreshButton = styled.button`
  margin-left: auto;
  padding: 0.45rem 0.85rem;
  border-radius: ${({ theme }) => theme.radii.sm};
  border: 1px solid ${({ theme }) => theme.colors.border};
  background: ${({ theme }) => theme.colors.surface};
  color: ${({ theme }) => theme.colors.text};
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;

  &:hover:not(:disabled) {
    background: ${({ theme }) => theme.colors.surfaceHover};
  }

  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
`;

export const CanvasWrap = styled.div`
  position: relative;
  flex: 1;
  min-height: 18rem;
  background:
    radial-gradient(circle at 50% 50%, color-mix(in srgb, ${({ theme }) => theme.colors.surface} 80%, transparent), transparent 70%),
    ${({ theme }) => theme.colors.surfaceMuted};
  border-radius: 0 0 ${({ theme }) => theme.radii.lg} ${({ theme }) => theme.radii.lg};
  overflow: hidden;
  touch-action: none;

  ${mediaUp('lg')} {
    border-radius: ${({ theme }) => theme.radii.lg};
    border: 1px solid ${({ theme }) => theme.colors.borderSubtle};
  }
`;

export const Canvas = styled.canvas`
  display: block;
  width: 100%;
  height: 100%;
  cursor: grab;

  &:active {
    cursor: grabbing;
  }
`;

export const GraphHint = styled.div`
  position: absolute;
  left: 0.75rem;
  bottom: 0.75rem;
  padding: 0.35rem 0.6rem;
  border-radius: ${({ theme }) => theme.radii.sm};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.bg} 72%, transparent);
  color: ${({ theme }) => theme.colors.textFaint};
  font-size: 0.72rem;
  pointer-events: none;
`;

export const StatsBar = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  padding: 0.6rem 1rem;
  font-size: 0.82rem;
  color: ${({ theme }) => theme.colors.textMuted};
  border-top: 1px solid ${({ theme }) => theme.colors.borderSubtle};
`;

export const DetailPanel = styled.aside<{ $open: boolean }>`
  display: ${({ $open }) => ($open ? 'flex' : 'none')};
  flex-direction: column;
  width: 100%;
  max-height: 40vh;
  border-top: 1px solid ${({ theme }) => theme.colors.borderSubtle};
  background: ${({ theme }) => theme.colors.bgElevated};
  overflow: hidden;

  ${mediaUp('lg')} {
    display: flex;
    width: 18rem;
    max-height: none;
    flex-shrink: 0;
    border-top: none;
    border-radius: ${({ theme }) => theme.radii.lg};
    border: 1px solid ${({ theme }) => theme.colors.borderSubtle};
    opacity: ${({ $open }) => ($open ? 1 : 0.6)};
  }
`;

export const DetailHeader = styled.div`
  padding: 1rem;
  border-bottom: 1px solid ${({ theme }) => theme.colors.borderSubtle};
`;

export const DetailTitle = styled.h2`
  margin: 0 0 0.35rem;
  font-size: 1rem;
  font-weight: 700;
  color: ${({ theme }) => theme.colors.text};
  line-height: 1.3;
`;

export const DetailMeta = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  font-size: 0.8rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const TypeBadge = styled.span<{ $color: string }>`
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.15rem 0.5rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  background: color-mix(in srgb, ${({ $color }) => $color} 18%, transparent);
  color: ${({ $color }) => $color};
  font-weight: 600;
`;

export const NeighborsList = styled.div`
  flex: 1;
  overflow: auto;
  padding: 0.5rem;
`;

export const NeighborItem = styled.button`
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  width: 100%;
  padding: 0.65rem 0.75rem;
  margin-bottom: 0.35rem;
  border: 1px solid transparent;
  border-radius: ${({ theme }) => theme.radii.sm};
  background: ${({ theme }) => theme.colors.surface};
  color: ${({ theme }) => theme.colors.text};
  text-align: left;
  cursor: pointer;

  &:hover {
    border-color: ${({ theme }) => theme.colors.border};
    background: ${({ theme }) => theme.colors.surfaceHover};
  }
`;

export const NeighborName = styled.span`
  font-size: 0.88rem;
  font-weight: 500;
`;

export const NeighborWeight = styled.span`
  flex-shrink: 0;
  font-size: 0.78rem;
  font-weight: 700;
  color: ${({ theme }) => theme.colors.accent};
`;

export const Legend = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem 0.85rem;
  padding: 0.5rem 1rem 0.75rem;
  border-top: 1px solid ${({ theme }) => theme.colors.borderSubtle};
`;

export const LegendItem = styled.span<{ $color: string }>`
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  font-size: 0.72rem;
  color: ${({ theme }) => theme.colors.textMuted};

  &::before {
    content: '';
    width: 0.55rem;
    height: 0.55rem;
    border-radius: 50%;
    background: ${({ $color }) => $color};
  }
`;

export const ErrorBanner = styled.div`
  margin: 0.75rem 1rem;
  padding: 0.75rem 1rem;
  border-radius: ${({ theme }) => theme.radii.md};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.danger} 12%, transparent);
  color: ${({ theme }) => theme.colors.danger};
  font-size: 0.9rem;
`;

export const LoadingOverlay = styled.div`
  position: absolute;
  inset: 0;
  z-index: 2;
  display: flex;
  align-items: center;
  justify-content: center;
  background: color-mix(in srgb, ${({ theme }) => theme.colors.bg} 50%, transparent);
`;

export const EmptyOverlay = styled.div`
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  text-align: center;
  color: ${({ theme }) => theme.colors.textMuted};
  font-size: 0.95rem;
  pointer-events: none;
`;
