using Com.Astral.GodotHub.Data;

namespace Com.Astral.GodotHub.Settings.Buttons.Toggles
{
	public partial class AutoUpdateToggle : SettingToggle
	{
		protected override void OnToggled(bool pToggled)
		{
			Config.AutoRefreshRepos = pToggled;
		}

		protected override void Reset()
		{
			button.ButtonPressed = Config.AutoRefreshRepos;
		}
	}
}
