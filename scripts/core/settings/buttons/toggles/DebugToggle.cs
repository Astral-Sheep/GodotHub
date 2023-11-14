using Com.Astral.GodotHub.Core.Data;
using Com.Astral.GodotHub.Core.Debug;

namespace Com.Astral.GodotHub.Core.Settings.Buttons.Toggles
{
	public partial class DebugToggle : SettingToggle
	{
		protected override void OnToggled(bool pToggled)
		{
			AppConfig.Debug = pToggled;
			Debugger.Enabled = pToggled;
		}

		protected override void Reset()
		{
			button.ButtonPressed = AppConfig.Debug;
		}
	}
}
