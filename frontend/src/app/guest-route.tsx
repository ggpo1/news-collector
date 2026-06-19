import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { LoadingState } from '../components/ui/loading-state';
import { useAuth } from '../contexts/auth-context';
import { PATHS } from './paths';

export function GuestRoute({ children }: { children: ReactNode }) {
  const { user, loading } = useAuth();

  if (loading) {
    return <LoadingState label="Проверка сессии…" />;
  }

  if (user) {
    return <Navigate to={PATHS.news} replace />;
  }

  return children;
}
