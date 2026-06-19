import { createGlobalStyle } from 'styled-components';

export const GlobalStyle = createGlobalStyle`
  *,
  *::before,
  *::after {
    box-sizing: border-box;
  }

  html {
    -webkit-text-size-adjust: 100%;
    text-size-adjust: 100%;
  }

  html,
  body,
  #root {
    margin: 0;
    min-height: 100%;
  }

  body {
    font-family: ${({ theme }) => theme.fonts.body};
    background:
      radial-gradient(ellipse 80% 50% at 50% -20%, color-mix(in srgb, ${({ theme }) => theme.colors.accent} 12%, transparent), transparent),
      radial-gradient(ellipse 60% 40% at 100% 100%, color-mix(in srgb, ${({ theme }) => theme.colors.accent} 6%, transparent), transparent),
      ${({ theme }) => theme.colors.bg};
    color: ${({ theme }) => theme.colors.text};
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
  }

  button,
  select,
  input,
  textarea {
    font: inherit;
  }

  button {
    touch-action: manipulation;
  }

  a {
    color: ${({ theme }) => theme.colors.accent};
  }

  :focus-visible {
    outline: 2px solid ${({ theme }) => theme.colors.accent};
    outline-offset: 2px;
  }

  ::selection {
    background: color-mix(in srgb, ${({ theme }) => theme.colors.accent} 35%, transparent);
  }
`;
