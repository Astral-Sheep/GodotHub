using Com.Astral.GodotHub.Data;

namespace Com.Astral.GodotHub.Settings.Buttons.Toggles
{
	public partial class AutoShortcutToggle : SettingToggle
	{
		protected override void OnToggled(bool pToggled)
		{
			Config.AutoCreateShortcut = pToggled;
		}

		protected override void Reset()
		{
			button.ButtonPressed = Config.AutoCreateShortcut;
		}
	}
}
