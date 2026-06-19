import { useState } from 'react';
import { deleteSource } from '../../api/client';
import type { Source } from '../../api/types';
import { EmptyState } from '../ui/empty-state';
import { LoadingState } from '../ui/loading-state';
import { SourceForm } from '../source-form/source-form';
import { SourcesList } from '../sources-list/sources-list';
import * as S from './sources-view.styles';

interface SourcesViewProps {
  sources: Source[];
  loading: boolean;
  onChanged: () => void;
}

export function SourcesView({ sources, loading, onChanged }: SourcesViewProps) {
  const [formOpen, setFormOpen] = useState(false);
  const [editingSource, setEditingSource] = useState<Source | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const openCreateForm = () => {
    setEditingSource(null);
    setFormOpen(true);
  };

  const openEditForm = (source: Source) => {
    setEditingSource(source);
    setFormOpen(true);
  };

  const closeForm = () => {
    setFormOpen(false);
    setEditingSource(null);
  };

  const handleDelete = async (source: Source) => {
    const confirmed = window.confirm(`Удалить источник «${source.name}»?`);
    if (!confirmed) {
      return;
    }

    setDeletingId(source.id);

    try {
      await deleteSource(source.id);
      onChanged();
    } catch (err) {
      window.alert(err instanceof Error ? err.message : 'Не удалось удалить источник');
    } finally {
      setDeletingId(null);
    }
  };

  return (
    <>
      <S.Root>
        <S.Toolbar>
          <S.AddButton type="button" onClick={openCreateForm}>
            + Добавить источник
          </S.AddButton>
        </S.Toolbar>

        {loading ? (
          <LoadingState label="Загрузка источников…" />
        ) : sources.length === 0 ? (
          <EmptyState>
            Источников пока нет. Добавьте RSS-фид, чтобы worker начал собирать новости.
          </EmptyState>
        ) : (
          <SourcesList
            items={sources}
            deletingId={deletingId}
            onEdit={openEditForm}
            onDelete={(source) => void handleDelete(source)}
          />
        )}
      </S.Root>

      {formOpen && (
        <SourceForm source={editingSource} onClose={closeForm} onSaved={onChanged} />
      )}
    </>
  );
}
