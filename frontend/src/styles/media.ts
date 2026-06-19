import type { AppTheme } from './theme';

type Breakpoint = keyof AppTheme['breakpoints'];

export function mediaUp(breakpoint: Breakpoint) {
  return (props: { theme: AppTheme }) =>
    `@media (min-width: ${props.theme.breakpoints[breakpoint]})`;
}

export function mediaDown(breakpoint: Breakpoint) {
  return (props: { theme: AppTheme }) =>
    `@media (max-width: calc(${props.theme.breakpoints[breakpoint]} - 1px))`;
}
