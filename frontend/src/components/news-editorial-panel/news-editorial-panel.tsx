import { useEffect, useState } from 'react';
import { updateNewsCategory, updateNewsEditorialTags } from '../../api/client';
import type { Category, EditorialTag, NewsItemDetail } from '../../api/types';
import { CategorySelect, UNCATEGORIZED_FILTER } from '../category-select/category-select';
import * as S from './news-editorial-panel.styles';

interface NewsEditorialPanelProps {
  item: NewsItemDetail;
  categories: Category[];
  tags: EditorialTag[];
  onUpdated: (item: NewsItemDetail) => void;
}

export function NewsEditorialPanel({ item, categories, tags, onUpdated }: NewsEditorialPanelProps) {
  const [categoryFilter, setCategoryFilter] = useState<string | null>(item.categoryId);
  const [selectedTagIds, setSelectedTagIds] = useState<string[]>(
    item.editorialTags.map((tag) => tag.id),
  );
  const [savingCategory, setSavingCategory] = useState(false);
  const [savingTags, setSavingTags] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setCategoryFilter(item.categoryId);
    setSelectedTagIds(item.editorialTags.map((tag) => tag.id));
  }, [item.id, item.categoryId, item.editorialTags]);

  const categoryDirty = item.categoryId !== (categoryFilter === UNCATEGORIZED_FILTER ? null : categoryFilter);

  const tagsDirty =
    [...selectedTagIds].sort().join(',') !==
    item.editorialTags
      .map((tag) => tag.id)
      .sort()
      .join(',');

  const handleSaveCategory = async () => {
    setSavingCategory(true);
    setError(null);
    try {
      const categoryId = categoryFilter === UNCATEGORIZED_FILTER ? null : categoryFilter;
      const updated = await updateNewsCategory(item.id, categoryId);
      onUpdated(updated);
      setCategoryFilter(updated.categoryId);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось сохранить категорию');
    } finally {
      setSavingCategory(false);
    }
  };

  const handleSaveTags = async () => {
    setSavingTags(true);
    setError(null);
    try {
      const updated = await updateNewsEditorialTags(item.id, selectedTagIds);
      onUpdated(updated);
      setSelectedTagIds(updated.editorialTags.map((tag) => tag.id));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось сохранить теги');
    } finally {
      setSavingTags(false);
    }
  };

  const toggleTag = (tagId: string) => {
    setSelectedTagIds((current) =>
      current.includes(tagId) ? current.filter((id) => id !== tagId) : [...current, tagId],
    );
  };

  return (
    <S.Panel>
      <S.Title>Редакционная разметка</S.Title>
      {item.isCategoryManual && <S.Hint>Категория зафиксирована вручную — автокатегоризатор не перезапишет.</S.Hint>}

      <S.Block>
        <CategorySelect
          categories={categories}
          value={categoryFilter}
          loading={false}
          onChange={setCategoryFilter}
        />
        <S.SaveButton type="button" disabled={!categoryDirty || savingCategory} onClick={() => void handleSaveCategory()}>
          {savingCategory ? 'Сохранение…' : 'Сохранить категорию'}
        </S.SaveButton>
      </S.Block>

      <S.Block>
        <S.Label>Редакционные теги</S.Label>
        <S.TagList>
          {tags.map((tag) => (
            <S.TagOption key={tag.id}>
              <input
                type="checkbox"
                checked={selectedTagIds.includes(tag.id)}
                onChange={() => toggleTag(tag.id)}
              />
              <span style={tag.color ? { color: tag.color } : undefined}>{tag.name}</span>
            </S.TagOption>
          ))}
        </S.TagList>
        <S.SaveButton type="button" disabled={!tagsDirty || savingTags} onClick={() => void handleSaveTags()}>
          {savingTags ? 'Сохранение…' : 'Сохранить теги'}
        </S.SaveButton>
      </S.Block>

      {error && <S.Error>{error}</S.Error>}
    </S.Panel>
  );
}
