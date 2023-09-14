using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Debug;

namespace Com.Astral.GodotHub.Settings.Buttons.Toggles
{
	public partial class DebugToggle : SettingToggle
	{
		protected override void OnToggled(bool pToggled)
		{
			Config.Debug = pToggled;
			Debugger.Enabled = pToggled;
		}

		protected override void Reset()
		{
			button.ButtonPressed = Config.Debug;
		}
	}
}
