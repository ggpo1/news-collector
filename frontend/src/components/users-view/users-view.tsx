import { useCallback, useEffect, useState } from 'react';
import { USER_ROLE_LABELS } from '../../api/role-labels';
import { createUser, getUsers } from '../../api/client';
import type { UserAccount, UserRole } from '../../api/types';
import { EmptyState } from '../ui/empty-state';
import { LoadingState } from '../ui/loading-state';
import * as S from './users-view.styles';

export function UsersView() {
  const [users, setUsers] = useState<UserAccount[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [login, setLogin] = useState('');
  const [password, setPassword] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [role, setRole] = useState<UserRole>('Editor');

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      setUsers(await getUsers());
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось загрузить пользователей');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  const resetForm = () => {
    setLogin('');
    setPassword('');
    setDisplayName('');
    setRole('Editor');
    setFormError(null);
  };

  const handleCreate = async () => {
    if (!login.trim() || !password.trim() || !displayName.trim()) {
      setFormError('Заполните все поля');
      return;
    }

    setSubmitting(true);
    setFormError(null);

    try {
      await createUser({
        login: login.trim(),
        password,
        displayName: displayName.trim(),
        role,
      });
      setFormOpen(false);
      resetForm();
      await load();
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Не удалось создать пользователя');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <>
      <S.Root>
        <S.Toolbar>
          <S.AddButton type="button" onClick={() => setFormOpen(true)}>
            + Добавить редактора
          </S.AddButton>
        </S.Toolbar>

        {loading ? (
          <LoadingState label="Загрузка пользователей…" />
        ) : error ? (
          <EmptyState error>{error}</EmptyState>
        ) : users.length === 0 ? (
          <EmptyState>Пользователей пока нет</EmptyState>
        ) : (
          <S.List>
            {users.map((user) => (
              <S.Card key={user.id}>
                <S.CardHeader>
                  <div>
                    <S.Name>{user.displayName}</S.Name>
                    <S.Login>@{user.login}</S.Login>
                  </div>
                  <S.Badge $muted={!user.isActive}>
                    {user.isActive ? USER_ROLE_LABELS[user.role] : 'отключён'}
                  </S.Badge>
                </S.CardHeader>
              </S.Card>
            ))}
          </S.List>
        )}
      </S.Root>

      {formOpen && (
        <S.Overlay role="presentation" onClick={() => setFormOpen(false)}>
          <S.Dialog onClick={(event) => event.stopPropagation()}>
            <S.Title>Новый пользователь</S.Title>

            <S.Field>
              Логин
              <S.Input value={login} disabled={submitting} onChange={(e) => setLogin(e.target.value)} />
            </S.Field>

            <S.Field>
              Пароль
              <S.Input
                type="password"
                value={password}
                disabled={submitting}
                onChange={(e) => setPassword(e.target.value)}
              />
            </S.Field>

            <S.Field>
              Имя
              <S.Input
                value={displayName}
                disabled={submitting}
                onChange={(e) => setDisplayName(e.target.value)}
              />
            </S.Field>

            <S.Field>
              Роль
              <S.Select
                value={role}
                disabled={submitting}
                onChange={(e) => setRole(e.target.value as UserRole)}
              >
                <option value="Editor">{USER_ROLE_LABELS.Editor}</option>
                <option value="ChiefEditor">{USER_ROLE_LABELS.ChiefEditor}</option>
              </S.Select>
            </S.Field>

            {formError && <S.Error>{formError}</S.Error>}

            <S.Actions>
              <S.Button type="button" disabled={submitting} onClick={() => setFormOpen(false)}>
                Отмена
              </S.Button>
              <S.PrimaryButton type="button" disabled={submitting} onClick={() => void handleCreate()}>
                {submitting ? 'Создание…' : 'Создать'}
              </S.PrimaryButton>
            </S.Actions>
          </S.Dialog>
        </S.Overlay>
      )}
    </>
  );
}
