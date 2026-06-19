import { useEffect, useState } from 'react';
import type { AppTheme } from '../../styles/theme';
import { theme } from '../../styles/theme';

type Breakpoint = keyof AppTheme['breakpoints'];

function getQuery(breakpoint: Breakpoint, mode: 'up' | 'down') {
  const width = theme.breakpoints[breakpoint];
  return mode === 'up'
    ? `(min-width: ${width})`
    : `(max-width: calc(${width} - 1px))`;
}

export function useMediaQuery(breakpoint: Breakpoint, mode: 'up' | 'down' = 'up') {
  const query = getQuery(breakpoint, mode);

  const [matches, setMatches] = useState(() => {
    if (typeof window === 'undefined') {
      return false;
    }

    return window.matchMedia(query).matches;
  });

  useEffect(() => {
    const media = window.matchMedia(query);
    const onChange = () => setMatches(media.matches);

    onChange();
    media.addEventListener('change', onChange);

    return () => media.removeEventListener('change', onChange);
  }, [query]);

  return matches;
}
