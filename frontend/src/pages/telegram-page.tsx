import * as SectionS from '../components/section-page/section-page.styles';
import { TelegramSettingsView } from '../components/telegram-settings-view/telegram-settings-view';

export function TelegramPage() {
  return (
    <SectionS.SectionPage>
      <TelegramSettingsView />
    </SectionS.SectionPage>
  );
}
