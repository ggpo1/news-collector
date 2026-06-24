import { useState } from 'react';
import { EditorialDashboardView } from '../components/editorial-dashboard/editorial-dashboard';
import * as SectionS from '../components/section-page/section-page.styles';
import {
  useEditorialDashboard,
  type DashboardWindowHours,
} from '../hooks/use-editorial-dashboard/use-editorial-dashboard';

export function DashboardPage() {
  const [windowHours, setWindowHours] = useState<DashboardWindowHours>(48);
  const { dashboard, loading, error, reload } = useEditorialDashboard(windowHours);

  return (
    <SectionS.SectionPage>
      <EditorialDashboardView
        dashboard={dashboard}
        loading={loading}
        error={error}
        windowHours={windowHours}
        onWindowHoursChange={setWindowHours}
        onReload={() => void reload()}
      />
    </SectionS.SectionPage>
  );
}
