import { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import type { StoryStatus } from '../api/types';
import { MasterDetailLayout } from '../components/master-detail-layout/master-detail-layout';
import { Pagination } from '../components/pagination/pagination';
import { StoryDetailView } from '../components/story-detail/story-detail';
import { StoryList } from '../components/story-list/story-list';
import * as SectionS from '../components/section-page/section-page.styles';
import { STORY_STATUS_OPTIONS } from '../api/story-labels';
import { useStories } from '../hooks/use-stories/use-stories';
import * as S from './stories-page.styles';

export function StoriesPage() {
  const location = useLocation();
  const locationStoryId = (location.state as { storyId?: string } | null)?.storyId ?? null;
  const [statusFilter, setStatusFilter] = useState<StoryStatus | null>(null);
  const [selectedStoryId, setSelectedStoryId] = useState<string | null>(locationStoryId);

  useEffect(() => {
    if (locationStoryId) {
      setSelectedStoryId(locationStoryId);
    }
  }, [locationStoryId]);
  const { items, page, totalPages, totalCount, loading, error, setPage } = useStories(statusFilter);

  return (
    <SectionS.SectionPage>
      <MasterDetailLayout
        stickyList
        detailOpen={Boolean(selectedStoryId)}
        onBack={() => setSelectedStoryId(null)}
        backLabel="К списку тем"
        listHeader={
          <S.FiltersRow>
            <label>
              <S.FilterLabel>Статус</S.FilterLabel>
              <S.Select
                value={statusFilter ?? ''}
                onChange={(e) => {
                  setSelectedStoryId(null);
                  setStatusFilter(e.target.value === '' ? null : (e.target.value as StoryStatus));
                }}
              >
                {STORY_STATUS_OPTIONS.map((option) => (
                  <option key={option.value || 'all'} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </S.Select>
            </label>
          </S.FiltersRow>
        }
        listBody={
          <StoryList
            items={items}
            loading={loading}
            error={error}
            selectedId={selectedStoryId}
            onSelect={setSelectedStoryId}
          />
        }
        listFooter={
          <Pagination
            embedded
            page={page}
            totalPages={totalPages}
            totalCount={totalCount}
            onPageChange={setPage}
          />
        }
        detail={<StoryDetailView storyId={selectedStoryId} />}
      />
    </SectionS.SectionPage>
  );
}
