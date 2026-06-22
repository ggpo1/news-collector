import { Outlet, useLocation } from 'react-router-dom';
import { AppShell } from '../components/app-shell/app-shell';
import { sectionFromPathname } from './paths';

const SECTION_META = {
  news: {
    title: 'Новости',
    subtitle: 'Лента по выбранному источнику',
  },
  links: {
    title: 'Связи',
    subtitle: 'Новости на одну тему из разных источников',
  },
  rewrites: {
    title: 'Переписи',
    subtitle: 'Сохранённые AI-версии новостей',
  },
  map: {
    title: 'Семантическая карта',
    subtitle: 'Связи персон, компаний и стран по совместным упоминаниям',
  },
  sources: {
    title: 'Источники',
    subtitle: 'RSS-фиды и настройки сбора',
  },
  users: {
    title: 'Пользователи',
    subtitle: 'Учётные записи редакторов',
  },
} as const;

export function AppLayout() {
  const { pathname } = useLocation();
  const section = sectionFromPathname(pathname);
  const meta = SECTION_META[section];

  return (
    <AppShell section={section} sectionTitle={meta.title} sectionSubtitle={meta.subtitle}>
      <Outlet />
    </AppShell>
  );
}
