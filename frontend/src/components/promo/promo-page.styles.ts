import styled, { keyframes } from 'styled-components';
import { mediaUp } from '../../styles/media';

const float = keyframes`
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-6px); }
`;

export const Page = styled.div`
  min-height: 100dvh;
  overflow-x: hidden;
`;

export const Header = styled.header`
  position: sticky;
  top: 0;
  z-index: 20;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.85rem 1.25rem;
  border-bottom: 1px solid ${({ theme }) => theme.colors.borderSubtle};
  background: color-mix(in srgb, ${({ theme }) => theme.colors.bg} 82%, transparent);
  backdrop-filter: blur(12px);

  ${mediaUp('md')} {
    padding: 0.95rem 2rem;
  }
`;

export const Logo = styled.div`
  display: flex;
  align-items: center;
  gap: 0.55rem;
  font-weight: 700;
  font-size: 1rem;
  letter-spacing: -0.02em;
`;

export const LogoMark = styled.span`
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  border-radius: ${({ theme }) => theme.radii.sm};
  background: linear-gradient(135deg, ${({ theme }) => theme.colors.accent}, #4361ee);
  color: #fff;
  font-size: 0.75rem;
  font-weight: 800;
`;

export const HeaderActions = styled.div`
  display: flex;
  align-items: center;
  gap: 0.5rem;
`;

export const GhostButton = styled.a`
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 2.5rem;
  padding: 0 0.9rem;
  border-radius: ${({ theme }) => theme.radii.md};
  border: 1px solid transparent;
  color: ${({ theme }) => theme.colors.textMuted};
  font-size: 0.88rem;
  font-weight: 600;
  text-decoration: none;
  transition: color ${({ theme }) => theme.transitions.fast};

  &:hover {
    color: ${({ theme }) => theme.colors.text};
  }
`;

export const PrimaryButton = styled.a`
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 2.5rem;
  padding: 0 1rem;
  border-radius: ${({ theme }) => theme.radii.md};
  border: 1px solid ${({ theme }) => theme.colors.accent};
  background: ${({ theme }) => theme.colors.accentMuted};
  color: ${({ theme }) => theme.colors.accent};
  font-size: 0.88rem;
  font-weight: 700;
  text-decoration: none;
  transition:
    transform ${({ theme }) => theme.transitions.fast},
    box-shadow ${({ theme }) => theme.transitions.fast};

  &:hover {
    transform: translateY(-1px);
    box-shadow: 0 8px 24px color-mix(in srgb, ${({ theme }) => theme.colors.accent} 25%, transparent);
  }
`;

export const Hero = styled.section`
  position: relative;
  padding: 3rem 1.25rem 2.5rem;
  max-width: ${({ theme }) => theme.layout.maxContentWidth};
  margin: 0 auto;

  ${mediaUp('md')} {
    padding: 4.5rem 2rem 3.5rem;
    display: grid;
    grid-template-columns: 1.1fr 0.9fr;
    gap: 2.5rem;
    align-items: center;
  }
`;

export const HeroGlow = styled.div`
  position: absolute;
  top: -4rem;
  left: 50%;
  transform: translateX(-50%);
  width: min(720px, 90vw);
  height: 320px;
  background: radial-gradient(
    ellipse at center,
    color-mix(in srgb, ${({ theme }) => theme.colors.accent} 22%, transparent),
    transparent 70%
  );
  pointer-events: none;
  z-index: 0;
`;

export const HeroContent = styled.div`
  position: relative;
  z-index: 1;
`;

export const Eyebrow = styled.p`
  margin: 0 0 0.75rem;
  font-size: 0.78rem;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  color: ${({ theme }) => theme.colors.accent};
`;

export const HeroTitle = styled.h1`
  margin: 0;
  font-size: clamp(2rem, 5vw, 3.15rem);
  line-height: 1.08;
  letter-spacing: -0.04em;
  font-weight: 700;
  max-width: 14ch;

  span {
    background: linear-gradient(135deg, ${({ theme }) => theme.colors.text}, ${({ theme }) => theme.colors.accent});
    -webkit-background-clip: text;
    background-clip: text;
    color: transparent;
  }
`;

export const HeroLead = styled.p`
  margin: 1.1rem 0 0;
  max-width: 36rem;
  font-size: 1.05rem;
  line-height: 1.6;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const HeroActions = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.65rem;
  margin-top: 1.75rem;
`;

export const HeroPrimary = styled(PrimaryButton)`
  min-height: 3rem;
  padding: 0 1.35rem;
  font-size: 0.95rem;
`;

export const HeroSecondary = styled(GhostButton)`
  min-height: 3rem;
  padding: 0 1.1rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.md};
  color: ${({ theme }) => theme.colors.text};
`;

export const HeroVisual = styled.div`
  position: relative;
  z-index: 1;
  margin-top: 2rem;

  ${mediaUp('md')} {
    margin-top: 0;
  }
`;

export const PreviewCard = styled.div`
  padding: 1.15rem;
  border: 1px solid ${({ theme }) => theme.colors.border};
  border-radius: ${({ theme }) => theme.radii.lg};
  background: ${({ theme }) => theme.colors.surface};
  box-shadow: ${({ theme }) => theme.shadows.lg};
  animation: ${float} 5s ease-in-out infinite;
`;

export const PreviewHeader = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.85rem;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const PreviewBadge = styled.span`
  display: inline-flex;
  padding: 0.2rem 0.5rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  background: color-mix(in srgb, #e85d04 14%, transparent);
  color: ${({ theme }) => theme.colors.text};
  font-size: 0.72rem;
  font-weight: 700;
`;

export const PreviewItem = styled.div`
  padding: 0.85rem 0;
  border-top: 1px solid ${({ theme }) => theme.colors.borderSubtle};

  &:first-of-type {
    border-top: 0;
    padding-top: 0;
  }
`;

export const PreviewTitle = styled.div`
  font-weight: 600;
  line-height: 1.35;
  margin-bottom: 0.35rem;
`;

export const PreviewMeta = styled.div`
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Stats = styled.section`
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 0.75rem;
  max-width: ${({ theme }) => theme.layout.maxContentWidth};
  margin: 0 auto;
  padding: 0 1.25rem 2.5rem;

  ${mediaUp('md')} {
    grid-template-columns: repeat(4, 1fr);
    padding: 0 2rem 3rem;
    gap: 1rem;
  }
`;

export const StatCard = styled.div`
  padding: 1rem 1.1rem;
  border: 1px solid ${({ theme }) => theme.colors.borderSubtle};
  border-radius: ${({ theme }) => theme.radii.md};
  background: ${({ theme }) => theme.colors.bgElevated};
`;

export const StatValue = styled.div`
  font-size: 1.35rem;
  font-weight: 700;
  letter-spacing: -0.03em;
  color: ${({ theme }) => theme.colors.accent};
`;

export const StatLabel = styled.div`
  margin-top: 0.25rem;
  font-size: 0.78rem;
  line-height: 1.4;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Section = styled.section`
  max-width: ${({ theme }) => theme.layout.maxContentWidth};
  margin: 0 auto;
  padding: 0 1.25rem 3.5rem;

  ${mediaUp('md')} {
    padding: 0 2rem 4rem;
  }
`;

export const SectionHeader = styled.div`
  margin-bottom: 1.5rem;

  ${mediaUp('md')} {
    margin-bottom: 2rem;
  }
`;

export const SectionTitle = styled.h2`
  margin: 0;
  font-size: clamp(1.45rem, 3vw, 2rem);
  letter-spacing: -0.03em;
`;

export const SectionLead = styled.p`
  margin: 0.55rem 0 0;
  max-width: 40rem;
  color: ${({ theme }) => theme.colors.textMuted};
  line-height: 1.55;
`;

export const FeatureGrid = styled.div`
  display: grid;
  gap: 0.85rem;

  ${mediaUp('md')} {
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 1rem;
  }

  ${mediaUp('lg')} {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
`;

export const FeatureCard = styled.article<{ $accent?: string }>`
  padding: 1.15rem 1.2rem;
  border: 1px solid ${({ theme }) => theme.colors.borderSubtle};
  border-radius: ${({ theme }) => theme.radii.lg};
  background: ${({ theme }) => theme.colors.surface};
  transition:
    border-color ${({ theme }) => theme.transitions.normal},
    transform ${({ theme }) => theme.transitions.normal};

  &:hover {
    border-color: color-mix(in srgb, ${({ $accent, theme }) => $accent ?? theme.colors.accent} 45%, ${({ theme }) => theme.colors.border});
    transform: translateY(-2px);
  }
`;

export const FeatureIcon = styled.div<{ $color?: string }>`
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2.35rem;
  height: 2.35rem;
  margin-bottom: 0.85rem;
  border-radius: ${({ theme }) => theme.radii.sm};
  background: color-mix(in srgb, ${({ $color, theme }) => $color ?? theme.colors.accent} 14%, transparent);
  color: ${({ $color, theme }) => $color ?? theme.colors.accent};
  font-size: 1.1rem;
`;

export const FeatureTitle = styled.h3`
  margin: 0 0 0.45rem;
  font-size: 1rem;
  font-weight: 700;
`;

export const FeatureText = styled.p`
  margin: 0;
  font-size: 0.88rem;
  line-height: 1.55;
  color: ${({ theme }) => theme.colors.textMuted};
`;

export const Workflow = styled.div`
  display: grid;
  gap: 1rem;

  ${mediaUp('lg')} {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
`;

export const WorkflowStep = styled.div`
  position: relative;
  padding: 1.25rem;
  border: 1px solid ${({ theme }) => theme.colors.borderSubtle};
  border-radius: ${({ theme }) => theme.radii.lg};
  background: linear-gradient(
    180deg,
    ${({ theme }) => theme.colors.bgElevated},
    ${({ theme }) => theme.colors.surfaceMuted}
  );
`;

export const StepNumber = styled.div`
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 1.75rem;
  height: 1.75rem;
  margin-bottom: 0.75rem;
  border-radius: ${({ theme }) => theme.radii.pill};
  background: ${({ theme }) => theme.colors.accentMuted};
  color: ${({ theme }) => theme.colors.accent};
  font-size: 0.82rem;
  font-weight: 800;
`;

export const AudienceGrid = styled.div`
  display: grid;
  gap: 1rem;

  ${mediaUp('md')} {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
`;

export const AudienceCard = styled.div`
  padding: 1.35rem;
  border-radius: ${({ theme }) => theme.radii.lg};
  border: 1px solid ${({ theme }) => theme.colors.border};
  background: ${({ theme }) => theme.colors.surface};
`;

export const AudienceTitle = styled.h3`
  margin: 0 0 0.75rem;
  font-size: 1.05rem;
`;

export const AudienceList = styled.ul`
  margin: 0;
  padding: 0;
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 0.55rem;
`;

export const AudienceItem = styled.li`
  display: flex;
  gap: 0.55rem;
  font-size: 0.88rem;
  line-height: 1.45;
  color: ${({ theme }) => theme.colors.textMuted};

  &::before {
    content: '✓';
    flex-shrink: 0;
    color: ${({ theme }) => theme.colors.success};
    font-weight: 700;
  }
`;

export const Cta = styled.section`
  max-width: ${({ theme }) => theme.layout.maxContentWidth};
  margin: 0 auto 2rem;
  padding: 0 1.25rem 3rem;

  ${mediaUp('md')} {
    padding: 0 2rem 4rem;
  }
`;

export const CtaCard = styled.div`
  padding: 2rem 1.5rem;
  border-radius: ${({ theme }) => theme.radii.lg};
  border: 1px solid color-mix(in srgb, ${({ theme }) => theme.colors.accent} 35%, ${({ theme }) => theme.colors.border});
  background:
    radial-gradient(ellipse 80% 80% at 0% 0%, color-mix(in srgb, ${({ theme }) => theme.colors.accent} 12%, transparent), transparent),
    ${({ theme }) => theme.colors.surface};
  text-align: center;

  ${mediaUp('md')} {
    padding: 2.75rem 2.5rem;
  }
`;

export const CtaTitle = styled.h2`
  margin: 0;
  font-size: clamp(1.35rem, 3vw, 1.85rem);
  letter-spacing: -0.03em;
`;

export const CtaLead = styled.p`
  margin: 0.75rem auto 0;
  max-width: 32rem;
  color: ${({ theme }) => theme.colors.textMuted};
  line-height: 1.55;
`;

export const CtaActions = styled.div`
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 0.65rem;
  margin-top: 1.5rem;
`;

export const Footer = styled.footer`
  padding: 1.25rem;
  border-top: 1px solid ${({ theme }) => theme.colors.borderSubtle};
  text-align: center;
  font-size: 0.78rem;
  color: ${({ theme }) => theme.colors.textFaint};
`;
