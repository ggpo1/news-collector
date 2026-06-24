import type { AppSection } from './app-nav';

interface NavIconProps {
  section: AppSection;
}

export function NavIcon({ section }: NavIconProps) {
  switch (section) {
    case 'news':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path
            d="M4 6.5A2.5 2.5 0 0 1 6.5 4h11A2.5 2.5 0 0 1 20 6.5v11A2.5 2.5 0 0 1 17.5 20h-11A2.5 2.5 0 0 1 4 17.5v-11Z"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.75"
          />
          <path d="M8 8h8M8 12h8M8 16h5" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" />
        </svg>
      );
    case 'links':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path
            d="M9.2 14.8a3.5 3.5 0 0 0 4.95 0l2.12-2.12a3.5 3.5 0 1 0-4.95-4.95l-1.06 1.06"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.75"
            strokeLinecap="round"
          />
          <path
            d="M14.8 9.2a3.5 3.5 0 0 0-4.95 0L7.73 11.3a3.5 3.5 0 0 0 4.95 4.95l1.06-1.06"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.75"
            strokeLinecap="round"
          />
        </svg>
      );
    case 'rewrites':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path
            d="M12 20h7.5a1.5 1.5 0 0 0 1.5-1.5V6.5A1.5 1.5 0 0 0 19.5 5H6.5A1.5 1.5 0 0 0 5 6.5V14"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.75"
            strokeLinecap="round"
          />
          <path
            d="M8.5 17.5 16 10l2 2-7.5 7.5H6v-2.5Z"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.75"
            strokeLinejoin="round"
          />
        </svg>
      );
    case 'map':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <circle cx="6.5" cy="17.5" r="2.25" fill="none" stroke="currentColor" strokeWidth="1.75" />
          <circle cx="17.5" cy="6.5" r="2.25" fill="none" stroke="currentColor" strokeWidth="1.75" />
          <circle cx="12" cy="12" r="2.25" fill="none" stroke="currentColor" strokeWidth="1.75" />
          <path
            d="M8.4 15.8 10.2 13.4M13.8 10.6l1.8-2.4M8.7 16.1l3.3-1.1M13.3 7.9l3.3-1.1"
            stroke="currentColor"
            strokeWidth="1.75"
            strokeLinecap="round"
          />
        </svg>
      );
    case 'sources':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <circle cx="12" cy="12" r="2.75" fill="none" stroke="currentColor" strokeWidth="1.75" />
          <path
            d="M12 3v2.25M12 18.75V21M3 12h2.25M18.75 12H21M5.3 5.3l1.59 1.59M17.11 17.11l1.59 1.59M5.3 18.7l1.59-1.59M17.11 6.89l1.59-1.59"
            stroke="currentColor"
            strokeWidth="1.75"
            strokeLinecap="round"
          />
        </svg>
      );
    case 'telegram':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path
            d="M21.5 4.5 3.8 11.1c-.9.35-.88 1.62.03 1.94l4.55 1.43 1.74 5.3c.28.86 1.43 1.05 1.96.34l2.5-3.2 4.65 3.43c.77.57 1.86.14 2.07-.82L22.7 6.2c.22-1.05-.62-1.95-1.7-1.7Z"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.75"
            strokeLinejoin="round"
          />
          <path d="m9.5 13.2 8.7-5.4" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" />
        </svg>
      );
    case 'users':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <circle cx="9" cy="8.5" r="3" fill="none" stroke="currentColor" strokeWidth="1.75" />
          <path
            d="M3.5 19.5c0-3 2.5-5 5.5-5s5.5 2 5.5 5"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.75"
            strokeLinecap="round"
          />
          <path
            d="M16 8.25h5M18.5 5.75v5"
            stroke="currentColor"
            strokeWidth="1.75"
            strokeLinecap="round"
          />
        </svg>
      );
    default:
      return null;
  }
}
