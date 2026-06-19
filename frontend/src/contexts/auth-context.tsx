import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { getAuthToken, setAuthToken } from '../api/auth-storage';
import {
  getCurrentUser,
  login as loginRequest,
  logoutLocally,
  register as registerRequest,
  setUnauthorizedHandler,
} from '../api/client';
import type { CurrentUser } from '../api/types';

interface AuthContextValue {
  user: CurrentUser | null;
  loading: boolean;
  login: (loginName: string, password: string) => Promise<void>;
  register: (
    invitationCode: string,
    loginName: string,
    password: string,
    displayName: string,
  ) => Promise<void>;
  logout: () => void;
  isChiefEditor: boolean;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [loading, setLoading] = useState(true);

  const logout = useCallback(() => {
    logoutLocally();
    setUser(null);
  }, []);

  const restoreSession = useCallback(async () => {
    const token = getAuthToken();
    if (!token) {
      setUser(null);
      setLoading(false);
      return;
    }

    try {
      const currentUser = await getCurrentUser();
      setUser(currentUser);
    } catch {
      logoutLocally();
      setUser(null);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    setUnauthorizedHandler(() => {
      logoutLocally();
      setUser(null);
    });

    void restoreSession();

    return () => setUnauthorizedHandler(null);
  }, [restoreSession]);

  const login = useCallback(async (loginName: string, password: string) => {
    const response = await loginRequest(loginName, password);
    setAuthToken(response.accessToken);
    setUser(response.user);
  }, []);

  const register = useCallback(
    async (invitationCode: string, loginName: string, password: string, displayName: string) => {
      const response = await registerRequest({
        invitationCode,
        login: loginName,
        password,
        displayName,
      });
      setAuthToken(response.accessToken);
      setUser(response.user);
    },
    [],
  );

  const value = useMemo(
    () => ({
      user,
      loading,
      login,
      register,
      logout,
      isChiefEditor: user?.role === 'ChiefEditor',
    }),
    [user, loading, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }

  return context;
}
