using Com.Astral.GodotHub.Data;

namespace Com.Astral.GodotHub.Settings.Buttons.Toggles
{
	public partial class AutoDeleteToggle : SettingToggle
	{
		protected override void OnToggled(bool pToggled)
		{
			AppConfig.AutoDeleteZip = pToggled;
		}

		protected override void Reset()
		{
			button.ButtonPressed = AppConfig.AutoDeleteZip;
		}
	}
}
