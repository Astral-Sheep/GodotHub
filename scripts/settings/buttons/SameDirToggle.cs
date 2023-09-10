using Godot;

namespace Com.Astral.GodotHub.Settings.Buttons
{
	public partial class SameDirToggle : SettingButton
	{
		[Export] protected DownloadDirButton downloadDirButton;

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
			Config.UseInstallDirForDownload = pToggled;
			downloadDirButton.Enabled = !pToggled;
		}

		protected override void Reset()
		{
			button.ButtonPressed = Config.UseInstallDirForDownload;
		}
	}
}
