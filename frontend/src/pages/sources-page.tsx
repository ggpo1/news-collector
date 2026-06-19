import { SourcesView } from '../components/sources-view/sources-view';
import { useSources } from '../hooks/use-sources/use-sources';

export function SourcesPage() {
  const { sources, loading, error, reload } = useSources();

  return (
    <>
      {error && <p role="alert">{error}</p>}
      <SourcesView sources={sources} loading={loading} onChanged={() => void reload()} />
    </>
  );
}
