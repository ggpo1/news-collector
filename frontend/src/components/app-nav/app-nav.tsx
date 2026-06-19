import { NavIcon } from './nav-icon';
import { useAuth } from '../../contexts/auth-context';
import * as S from './app-nav.styles';

export type AppSection = 'news' | 'links' | 'rewrites' | 'sources' | 'users';

const NAV_ITEMS: { id: AppSection; label: string; chiefOnly?: boolean }[] = [
  { id: 'news', label: 'Новости' },
  { id: 'links', label: 'Связи' },
  { id: 'rewrites', label: 'Переписи' },
  { id: 'sources', label: 'Источники', chiefOnly: true },
  { id: 'users', label: 'Пользователи', chiefOnly: true },
];

interface AppNavProps {
  value: AppSection;
  onChange: (section: AppSection) => void;
  variant: 'sidebar' | 'bottom';
}

export function AppNav({ value, onChange, variant }: AppNavProps) {
  const { isChiefEditor } = useAuth();
  const items = NAV_ITEMS.filter((item) => !item.chiefOnly || isChiefEditor);
  const Root = variant === 'sidebar' ? S.SidebarNav : S.BottomNav;

  return (
    <Root aria-label="Разделы приложения" $columns={items.length}>
      {items.map((item) => (
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
