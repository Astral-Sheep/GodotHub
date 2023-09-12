using Godot;

namespace Com.Astral.GodotHub.Settings.Buttons
{
	public partial class ResetButton : Button, ISettingButton
	{
		public void Connect()
		{
			Pressed += OnPressed;
		}

		public void Disconnect()
		{
			Pressed -= OnPressed;
		}

		protected void OnPressed()
		{
			Config.ResetAll();
		}
	}
}
