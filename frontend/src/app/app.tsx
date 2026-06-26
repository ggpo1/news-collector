import { Route, Routes } from 'react-router-dom';
import { NewsLinksView } from '../components/news-links-view/news-links-view';
import { LoginPage } from '../components/login-page/login-page';
import { RegisterDetailsPage } from '../components/login-page/register-details-page';
import { RegisterPage } from '../components/login-page/register-page';
import { UsersView } from '../components/users-view/users-view';
import { PromoPageRoute } from '../pages/promo-page';
import { BriefPage } from '../pages/brief-page';
import { DashboardPage } from '../pages/dashboard-page';
import { EntityMapPage } from '../pages/entity-map-page';
import { NewsPage } from '../pages/news-page';
import { StoriesPage } from '../pages/stories-page';
import { RewritesPage } from '../pages/rewrites-page';
import { SourcesPage } from '../pages/sources-page';
import { TelegramPage } from '../pages/telegram-page';
import { AppLayout } from './app-layout';
import { ChiefEditorRoute } from './chief-editor-route';
import { GuestRoute } from './guest-route';
import { NewsRootRedirect } from './news-root-redirect';
import { PATHS } from './paths';
import { NotFoundRedirect, RootRedirect } from './root-redirect';
import { RequireAuth } from './require-auth';

export default function App() {
  return (
    <Routes>
      <Route path={PATHS.promo} element={<PromoPageRoute />} />
      <Route
        path={PATHS.login}
        element={
          <GuestRoute>
            <LoginPage />
          </GuestRoute>
        }
      />
      <Route
        path={PATHS.register}
        element={
          <GuestRoute>
            <RegisterPage />
          </GuestRoute>
        }
      />
      <Route
        path={PATHS.registerDetails}
        element={
          <GuestRoute>
            <RegisterDetailsPage />
          </GuestRoute>
        }
      />

      <Route
        element={
          <RequireAuth>
            <AppLayout />
          </RequireAuth>
        }
      >
        <Route path={PATHS.dashboard} element={<DashboardPage />} />
        <Route path={PATHS.brief} element={<BriefPage />} />
        <Route path={PATHS.stories} element={<StoriesPage />} />
        <Route path={PATHS.news} element={<NewsRootRedirect />} />
        <Route path={`${PATHS.news}/:pageNum`} element={<NewsPage />} />
        <Route path={PATHS.links} element={<NewsLinksView />} />
        <Route path={PATHS.rewrites} element={<RewritesPage />} />
        <Route path={PATHS.map} element={<EntityMapPage />} />
        <Route
          path={PATHS.sources}
          element={
            <ChiefEditorRoute>
              <SourcesPage />
            </ChiefEditorRoute>
          }
        />
        <Route
          path={PATHS.telegram}
          element={
            <ChiefEditorRoute>
              <TelegramPage />
            </ChiefEditorRoute>
          }
        />
        <Route
          path={PATHS.users}
          element={
            <ChiefEditorRoute>
              <UsersView />
            </ChiefEditorRoute>
          }
        />
      </Route>

      <Route path="/" element={<RootRedirect />} />
      <Route path="*" element={<NotFoundRedirect />} />
    </Routes>
  );
}
