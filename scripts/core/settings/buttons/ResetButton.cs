using Com.Astral.GodotHub.Data;
using Godot;

namespace Com.Astral.GodotHub.Settings.Buttons
{
	public partial class ResetButton : Button, ISettingButton
	{
		public void Connect()
		{
			Pressed += AppConfig.ResetAll;
		}

		public void Disconnect()
		{
			Pressed -= AppConfig.ResetAll;
		}
	}
}
