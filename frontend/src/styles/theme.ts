export const theme = {
  colors: {
    bg: '#0b0f14',
    bgElevated: '#10161f',
    surface: '#151c28',
    surfaceHover: '#1c2534',
    surfaceMuted: '#121820',
    border: '#273244',
    borderSubtle: '#1e2836',
    text: '#eef2f7',
    textMuted: '#8b9cb3',
    textFaint: '#5c6d84',
    accent: '#5b9aff',
    accentMuted: 'color-mix(in srgb, #5b9aff 16%, transparent)',
    danger: '#ff6b7a',
    success: '#3dd68c',
  },
  breakpoints: {
    sm: '480px',
    md: '768px',
    lg: '1024px',
    xl: '1280px',
  },
  spacing: {
    xs: '0.25rem',
    sm: '0.5rem',
    md: '1rem',
    lg: '1.5rem',
    xl: '2rem',
  },
  radii: {
    sm: '10px',
    md: '14px',
    lg: '18px',
    pill: '999px',
  },
  shadows: {
    sm: '0 4px 16px rgba(0, 0, 0, 0.18)',
    md: '0 12px 32px rgba(0, 0, 0, 0.28)',
    lg: '0 24px 48px rgba(0, 0, 0, 0.36)',
  },
  layout: {
    sidebarWidth: '15.5rem',
    bottomNavHeight: '4.25rem',
    headerHeight: '3.75rem',
    maxContentWidth: '82rem',
  },
  fonts: {
    body: "'IBM Plex Sans', system-ui, -apple-system, sans-serif",
  },
  transitions: {
    fast: '150ms ease',
    normal: '220ms ease',
  },
} as const;

export type AppTheme = typeof theme;
