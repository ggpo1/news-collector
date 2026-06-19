import { useCallback, useEffect, useState } from 'react';
import { USER_ROLE_LABELS } from '../../api/role-labels';
import { createInvitationCode, createUser, getInvitationCodes, getUsers } from '../../api/client';
import type { InvitationCode, UserAccount, UserRole } from '../../api/types';
import { EmptyState } from '../ui/empty-state';
import { LoadingState } from '../ui/loading-state';
import * as S from './users-view.styles';

export function UsersView() {
  const [users, setUsers] = useState<UserAccount[]>([]);
  const [invitationCodes, setInvitationCodes] = useState<InvitationCode[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [inviteFormOpen, setInviteFormOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [login, setLogin] = useState('');
  const [password, setPassword] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [role, setRole] = useState<UserRole>('Editor');
  const [inviteRole, setInviteRole] = useState<UserRole>('Editor');
  const [createdCode, setCreatedCode] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const [usersResult, codesResult] = await Promise.all([getUsers(), getInvitationCodes()]);
      setUsers(usersResult);
      setInvitationCodes(codesResult);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось загрузить данные');
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

  const resetInviteForm = () => {
    setInviteRole('Editor');
    setCreatedCode(null);
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

  const handleCreateInvitation = async () => {
    setSubmitting(true);
    setFormError(null);

    try {
      const invitation = await createInvitationCode({ role: inviteRole });
      setCreatedCode(invitation.code);
      await load();
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Не удалось создать код');
    } finally {
      setSubmitting(false);
    }
  };

  const copyCode = async (code: string) => {
    try {
      await navigator.clipboard.writeText(code);
    } catch {
      // Clipboard may be unavailable.
    }
  };

  return (
    <>
      <S.Root>
        <S.Toolbar>
          <S.AddButton type="button" onClick={() => setInviteFormOpen(true)}>
            + Код приглашения
          </S.AddButton>
          <S.AddButton type="button" onClick={() => setFormOpen(true)}>
            + Добавить редактора
          </S.AddButton>
        </S.Toolbar>

        {loading ? (
          <LoadingState label="Загрузка…" />
        ) : error ? (
          <EmptyState error>{error}</EmptyState>
        ) : (
          <>
            <S.Section>
              <S.SectionTitle>Пригласительные коды</S.SectionTitle>
              {invitationCodes.length === 0 ? (
                <EmptyState>Активных кодов пока нет</EmptyState>
              ) : (
                <S.List>
                  {invitationCodes.map((item) => (
                    <S.Card key={item.code}>
                      <S.CardHeader>
                        <div>
                          <S.Code>{item.code}</S.Code>
                          <S.Login>
                            {USER_ROLE_LABELS[item.role]} · создал @{item.createdByLogin}
                          </S.Login>
                        </div>
                        <S.Badge $muted={Boolean(item.usedAt)}>
                          {item.usedAt
                            ? `использован @${item.usedByLogin ?? '—'}`
                            : 'ожидает'}
                        </S.Badge>
                      </S.CardHeader>
                      {!item.usedAt && (
                        <S.CopyButton type="button" onClick={() => void copyCode(item.code)}>
                          Скопировать код
                        </S.CopyButton>
                      )}
                    </S.Card>
                  ))}
                </S.List>
              )}
            </S.Section>

            <S.Section>
              <S.SectionTitle>Пользователи</S.SectionTitle>
              {users.length === 0 ? (
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
            </S.Section>
          </>
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

      {inviteFormOpen && (
        <S.Overlay role="presentation" onClick={() => setInviteFormOpen(false)}>
          <S.Dialog onClick={(event) => event.stopPropagation()}>
            <S.Title>Новый код приглашения</S.Title>

            {createdCode ? (
              <>
                <S.Hint>Отправьте этот код новому пользователю. Он одноразовый.</S.Hint>
                <S.CodeBlock>{createdCode}</S.CodeBlock>
                <S.Actions>
                  <S.Button type="button" onClick={() => void copyCode(createdCode)}>
                    Скопировать
                  </S.Button>
                  <S.PrimaryButton
                    type="button"
                    onClick={() => {
                      setInviteFormOpen(false);
                      resetInviteForm();
                    }}
                  >
                    Готово
                  </S.PrimaryButton>
                </S.Actions>
              </>
            ) : (
              <>
                <S.Field>
                  Роль для регистрации
                  <S.Select
                    value={inviteRole}
                    disabled={submitting}
                    onChange={(e) => setInviteRole(e.target.value as UserRole)}
                  >
                    <option value="Editor">{USER_ROLE_LABELS.Editor}</option>
                    <option value="ChiefEditor">{USER_ROLE_LABELS.ChiefEditor}</option>
                  </S.Select>
                </S.Field>

                {formError && <S.Error>{formError}</S.Error>}

                <S.Actions>
                  <S.Button
                    type="button"
                    disabled={submitting}
                    onClick={() => {
                      setInviteFormOpen(false);
                      resetInviteForm();
                    }}
                  >
                    Отмена
                  </S.Button>
                  <S.PrimaryButton
                    type="button"
                    disabled={submitting}
                    onClick={() => void handleCreateInvitation()}
                  >
                    {submitting ? 'Создание…' : 'Создать код'}
                  </S.PrimaryButton>
                </S.Actions>
              </>
            )}
          </S.Dialog>
        </S.Overlay>
      )}
    </>
  );
}
