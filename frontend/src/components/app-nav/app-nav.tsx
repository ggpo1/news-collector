import * as S from './app-nav.styles';

export type AppSection = 'news' | 'links' | 'rewrites' | 'sources';

interface AppNavProps {
  value: AppSection;
  onChange: (section: AppSection) => void;
}

export function AppNav({ value, onChange }: AppNavProps) {
  return (
    <S.Nav aria-label="Разделы приложения">
      <S.NavButton type="button" $active={value === 'news'} onClick={() => onChange('news')}>
        Новости
      </S.NavButton>
      <S.NavButton type="button" $active={value === 'links'} onClick={() => onChange('links')}>
        Связи
      </S.NavButton>
      <S.NavButton type="button" $active={value === 'rewrites'} onClick={() => onChange('rewrites')}>
        Переписи
      </S.NavButton>
      <S.NavButton type="button" $active={value === 'sources'} onClick={() => onChange('sources')}>
        Источники
      </S.NavButton>
    </S.Nav>
  );
}
