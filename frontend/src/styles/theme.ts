export const theme = {
  colors: {
    bg: '#0f1419',
    surface: '#1a2332',
    surfaceHover: '#222d3f',
    border: '#2d3a4f',
    text: '#e8edf4',
    textMuted: '#8b9cb3',
    accent: '#4f8cff',
    danger: '#f07178',
  },
  radii: {
    sm: '8px',
    md: '12px',
    pill: '999px',
  },
  fonts: {
    body: "'IBM Plex Sans', system-ui, sans-serif",
  },
} as const;

export type AppTheme = typeof theme;
