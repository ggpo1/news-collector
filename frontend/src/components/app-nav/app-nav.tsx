import { NavIcon } from './nav-icon';
import * as S from './app-nav.styles';

export type AppSection = 'news' | 'links' | 'rewrites' | 'sources';

const NAV_ITEMS: { id: AppSection; label: string }[] = [
  { id: 'news', label: 'Новости' },
  { id: 'links', label: 'Связи' },
  { id: 'rewrites', label: 'Переписи' },
  { id: 'sources', label: 'Источники' },
];

interface AppNavProps {
  value: AppSection;
  onChange: (section: AppSection) => void;
  variant: 'sidebar' | 'bottom';
}

export function AppNav({ value, onChange, variant }: AppNavProps) {
  const Root = variant === 'sidebar' ? S.SidebarNav : S.BottomNav;

  return (
    <Root aria-label="Разделы приложения">
      {NAV_ITEMS.map((item) => (
        <S.NavButton
          key={item.id}
          type="button"
          $active={value === item.id}
          $variant={variant}
          aria-current={value === item.id ? 'page' : undefined}
          onClick={() => onChange(item.id)}
        >
          <NavIcon section={item.id} />
          <S.NavLabel $hideOnMobile={variant === 'bottom'}>{item.label}</S.NavLabel>
        </S.NavButton>
      ))}
    </Root>
  );
}
