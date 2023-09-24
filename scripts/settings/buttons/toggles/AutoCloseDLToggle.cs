using Com.Astral.GodotHub.Data;

namespace Com.Astral.GodotHub.Settings.Buttons.Toggles
{
	public partial class AutoCloseDLToggle : SettingToggle
	{
		protected override void OnToggled(bool pToggled)
		{
			AppConfig.AutoCloseDownload = pToggled;
		}

		protected override void Reset()
		{
			button.ButtonPressed = AppConfig.AutoCloseDownload;
		}
	}
}
