import { useEffect, useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { USER_ROLE_LABELS } from '../../api/role-labels';
import type { UserRole } from '../../api/types';
import { useAuth } from '../../contexts/auth-context';
import { PATHS } from '../../app/paths';
import * as S from './login-page.styles';

interface RegisterDetailsState {
  invitationCode?: string;
  role?: UserRole;
}

export function RegisterDetailsPage() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const state = (location.state as RegisterDetailsState | null) ?? null;
  const [loginName, setLoginName] = useState('');
  const [password, setPassword] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!state?.invitationCode || !state.role) {
      navigate(PATHS.register, { replace: true });
    }
  }, [navigate, state?.invitationCode, state?.role]);

  if (!state?.invitationCode || !state.role) {
    return null;
  }

  const invitationCode = state.invitationCode;
  const invitationRole = state.role;

  const handleRegister = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      await register(invitationCode, loginName.trim(), password, displayName.trim());
      navigate(PATHS.news, { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось зарегистрироваться');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <S.Page>
      <S.Card>
        <S.Title>Создание аккаунта</S.Title>
        <S.Subtitle>Роль по приглашению: {USER_ROLE_LABELS[invitationRole]}</S.Subtitle>

        <S.Form onSubmit={(event) => void handleRegister(event)}>
          <S.Field>
            Логин
            <S.Input
              value={loginName}
              autoComplete="username"
              disabled={submitting}
              onChange={(event) => setLoginName(event.target.value)}
            />
          </S.Field>

          <S.Field>
            Имя
            <S.Input
              value={displayName}
              autoComplete="name"
              disabled={submitting}
              onChange={(event) => setDisplayName(event.target.value)}
            />
          </S.Field>

          <S.Field>
            Пароль
            <S.Input
              type="password"
              value={password}
              autoComplete="new-password"
              disabled={submitting}
              onChange={(event) => setPassword(event.target.value)}
            />
          </S.Field>

          {error && <S.Error>{error}</S.Error>}

          <S.Button type="submit" disabled={submitting}>
            {submitting ? 'Регистрация…' : 'Зарегистрироваться'}
          </S.Button>
        </S.Form>

        <S.Footer>
          <S.LinkButton as={Link} to={PATHS.register}>
            Назад к коду
          </S.LinkButton>
        </S.Footer>
      </S.Card>
    </S.Page>
  );
}
