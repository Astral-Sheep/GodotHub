using Com.Astral.GodotHub.Core.Data;

namespace Com.Astral.GodotHub.Core.Settings.Buttons.Toggles
{
	public partial class AutoShortcutToggle : SettingToggle
	{
		protected override void OnToggled(bool pToggled)
		{
			AppConfig.AutoCreateShortcut = pToggled;
		}

		protected override void Reset()
		{
			button.ButtonPressed = AppConfig.AutoCreateShortcut;
		}
	}
}
