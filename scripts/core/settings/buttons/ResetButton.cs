using Com.Astral.GodotHub.Core.Data;
using Godot;

namespace Com.Astral.GodotHub.Core.Settings.Buttons
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
