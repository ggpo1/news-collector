import { useMemo, useState } from 'react';
import type { LinkType } from '../../api/types';
import { useNewsLinks } from '../../hooks/use-news-links/use-news-links';
import { LinkTypeFilter } from '../link-type-filter/link-type-filter';
import { NewsLinkDetail } from '../news-link-detail/news-link-detail';
import { NewsLinksList } from '../news-links-list/news-links-list';
import { Pagination } from '../pagination/pagination';
import * as S from './news-links-view.styles';

export function NewsLinksView() {
  const [linkType, setLinkType] = useState<LinkType | null>(null);
  const [selectedLinkId, setSelectedLinkId] = useState<string | null>(null);

  const { items, page, totalPages, totalCount, loading, error, setPage } = useNewsLinks(linkType);

  const selectedLink = useMemo(
    () => items.find((item) => item.id === selectedLinkId) ?? null,
    [items, selectedLinkId],
  );

  const handleLinkTypeChange = (value: LinkType | null) => {
    setLinkType(value);
    setSelectedLinkId(null);
  };

  return (
    <S.Grid>
      <S.ListColumn>
        <LinkTypeFilter value={linkType} onChange={handleLinkTypeChange} />
        <NewsLinksList
          items={items}
          loading={loading}
          error={error}
          selectedId={selectedLinkId}
          onSelect={setSelectedLinkId}
        />
        <Pagination page={page} totalPages={totalPages} totalCount={totalCount} onPageChange={setPage} />
      </S.ListColumn>

      <NewsLinkDetail link={selectedLink} />
    </S.Grid>
  );
}
