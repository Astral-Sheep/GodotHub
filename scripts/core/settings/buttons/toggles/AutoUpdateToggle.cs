using Com.Astral.GodotHub.Core.Data;

namespace Com.Astral.GodotHub.Core.Settings.Buttons.Toggles
{
	public partial class AutoUpdateToggle : SettingToggle
	{
		protected override void OnToggled(bool pToggled)
		{
			AppConfig.AutoUpdateRepository = pToggled;
		}

		protected override void Reset()
		{
			button.ButtonPressed = AppConfig.AutoUpdateRepository;
		}
	}
}
