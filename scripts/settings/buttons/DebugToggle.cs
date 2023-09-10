using Com.Astral.GodotHub.Debug;

namespace Com.Astral.GodotHub.Settings.Buttons
{
	public partial class DebugToggle : SettingButton
	{
		public override void _Ready()
		{
			button.ButtonPressed = Config.Debug;
		}

		public override void Connect()
		{
			button.Toggled += OnToggled;
		}

		public override void Disconnect()
		{
			button.Toggled -= OnToggled;
		}

		protected void OnToggled(bool pToggled)
		{
			Config.Debug = pToggled;
			Debugger.Enabled = pToggled;
		}
	}
}