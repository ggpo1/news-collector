import { Navigate, Route, Routes } from 'react-router-dom';
import { NewsLinksView } from '../components/news-links-view/news-links-view';
import { LoginPage } from '../components/login-page/login-page';
import { RegisterDetailsPage } from '../components/login-page/register-details-page';
import { RegisterPage } from '../components/login-page/register-page';
import { UsersView } from '../components/users-view/users-view';
import { DashboardPage } from '../pages/dashboard-page';
import { EntityMapPage } from '../pages/entity-map-page';
import { NewsPage } from '../pages/news-page';
import { RewritesPage } from '../pages/rewrites-page';
import { SourcesPage } from '../pages/sources-page';
import { TelegramPage } from '../pages/telegram-page';
import { AppLayout } from './app-layout';
import { ChiefEditorRoute } from './chief-editor-route';
import { GuestRoute } from './guest-route';
import { PATHS } from './paths';
import { RequireAuth } from './require-auth';

export default function App() {
  return (
    <Routes>
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
        path="/"
        element={
          <RequireAuth>
            <AppLayout />
          </RequireAuth>
        }
      >
        <Route index element={<Navigate to="dashboard" replace />} />
        <Route path="dashboard" element={<DashboardPage />} />
        <Route path="news" element={<NewsPage />} />
        <Route path="links" element={<NewsLinksView />} />
        <Route path="rewrites" element={<RewritesPage />} />
        <Route path="map" element={<EntityMapPage />} />
        <Route
          path="sources"
          element={
            <ChiefEditorRoute>
              <SourcesPage />
            </ChiefEditorRoute>
          }
        />
        <Route
          path="telegram"
          element={
            <ChiefEditorRoute>
              <TelegramPage />
            </ChiefEditorRoute>
          }
        />
        <Route
          path="users"
          element={
            <ChiefEditorRoute>
              <UsersView />
            </ChiefEditorRoute>
          }
        />
      </Route>

      <Route path="*" element={<Navigate to={PATHS.news} replace />} />
    </Routes>
  );
}
