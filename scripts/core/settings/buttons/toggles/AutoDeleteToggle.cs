using Com.Astral.GodotHub.Core.Data;

namespace Com.Astral.GodotHub.Core.Settings.Buttons.Toggles
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
