import type { AppSection } from '../components/app-nav/app-nav';

export const PATHS = {
  login: '/login',
  register: '/register',
  registerDetails: '/register/details',
  dashboard: '/dashboard',
  brief: '/brief',
  stories: '/stories',
  news: '/news',
  links: '/links',
  rewrites: '/rewrites',
  map: '/map',
  sources: '/sources',
  telegram: '/telegram',
  users: '/users',
} as const;

export const SECTION_PATHS: Record<AppSection, string> = {
  dashboard: PATHS.dashboard,
  brief: PATHS.brief,
  stories: PATHS.stories,
  news: PATHS.news,
  links: PATHS.links,
  rewrites: PATHS.rewrites,
  map: PATHS.map,
  sources: PATHS.sources,
  telegram: PATHS.telegram,
  users: PATHS.users,
};

const PATH_TO_SECTION = Object.entries(SECTION_PATHS).reduce(
  (acc, [section, path]) => {
    acc[path] = section as AppSection;
    return acc;
  },
  {} as Record<string, AppSection>,
);

export function sectionFromPathname(pathname: string): AppSection {
  return PATH_TO_SECTION[pathname] ?? 'dashboard';
}
