using Com.Astral.GodotHub.Data;

namespace Com.Astral.GodotHub.Settings.Buttons.Toggles
{
	public partial class AutoCloseDLToggle : SettingToggle
	{
		protected override void OnToggled(bool pToggled)
		{
			Config.AutoCloseDownload = pToggled;
		}

		protected override void Reset()
		{
			button.ButtonPressed = Config.AutoCloseDownload;
		}
	}
}
