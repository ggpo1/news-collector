import { useMemo, useState } from 'react';
import type { LinkType } from '../../api/types';
import { useNewsLinks } from '../../hooks/use-news-links/use-news-links';
import { LinkTypeFilter } from '../link-type-filter/link-type-filter';
import { MasterDetailLayout } from '../master-detail-layout/master-detail-layout';
import { NewsLinkDetail } from '../news-link-detail/news-link-detail';
import { NewsLinksList } from '../news-links-list/news-links-list';
import { Pagination } from '../pagination/pagination';
import * as SectionS from '../section-page/section-page.styles';

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
    <SectionS.SectionPage>
      <MasterDetailLayout
        stickyList
        detailOpen={Boolean(selectedLinkId)}
        onBack={() => setSelectedLinkId(null)}
        backLabel="К списку связей"
        listHeader={<LinkTypeFilter value={linkType} onChange={handleLinkTypeChange} />}
        listBody={
          <NewsLinksList
            items={items}
            loading={loading}
            error={error}
            selectedId={selectedLinkId}
            onSelect={setSelectedLinkId}
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
        detail={<NewsLinkDetail link={selectedLink} />}
      />
    </SectionS.SectionPage>
  );
}
