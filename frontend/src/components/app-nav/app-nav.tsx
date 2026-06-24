import { SECTION_PATHS } from '../../app/paths';
import { useAuth } from '../../contexts/auth-context';
import { NavIcon } from './nav-icon';
import * as S from './app-nav.styles';

export type AppSection = 'news' | 'links' | 'rewrites' | 'map' | 'sources' | 'telegram' | 'users';

const NAV_ITEMS: { id: AppSection; label: string; chiefOnly?: boolean }[] = [
  { id: 'news', label: 'Новости' },
  { id: 'links', label: 'Связи' },
  { id: 'rewrites', label: 'Переписи' },
  { id: 'map', label: 'Карта' },
  { id: 'sources', label: 'Источники', chiefOnly: true },
  { id: 'telegram', label: 'Telegram', chiefOnly: true },
  { id: 'users', label: 'Пользователи', chiefOnly: true },
];

interface AppNavProps {
  section: AppSection;
  variant: 'sidebar' | 'bottom';
}

export function AppNav({ section, variant }: AppNavProps) {
  const { isChiefEditor } = useAuth();
  const items = NAV_ITEMS.filter((item) => !item.chiefOnly || isChiefEditor);
  const Root = variant === 'sidebar' ? S.SidebarNav : S.BottomNav;

  return (
    <Root aria-label="Разделы приложения" $columns={items.length}>
      {items.map((item) => (
        <S.NavLinkButton
          key={item.id}
          to={SECTION_PATHS[item.id]}
          $variant={variant}
          aria-current={section === item.id ? 'page' : undefined}
        >
          <NavIcon section={item.id} />
          <S.NavLabel $hideOnMobile={variant === 'bottom'}>{item.label}</S.NavLabel>
        </S.NavLinkButton>
      ))}
    </Root>
  );
}
