import styled from 'styled-components';

export const Layout = styled.div`
  max-width: 1200px;
  margin: 0 auto;
  padding: 1.5rem;
`;

export const Header = styled.header`
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
`;

export const Title = styled.h1`
  margin: 0;
  font-size: 1.5rem;
  font-weight: 700;
`;

export const Subtitle = styled.p`
  margin: 0.25rem 0 0;
  font-size: 0.9rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const ErrorBanner = styled.div`
  margin-bottom: 1rem;
  padding: 0.75rem 1rem;
  border-radius: ${({ theme }) => theme.radii.sm};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.danger} 12%, transparent);
  color: ${({ theme }) => theme.colors.danger};
  font-size: 0.9rem;
`;

export const Grid = styled.div`
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(280px, 380px);
  gap: 1.25rem;
  align-items: start;

  @media (max-width: 900px) {
    grid-template-columns: 1fr;
  }
`;

export const ListColumn = styled.section``;
