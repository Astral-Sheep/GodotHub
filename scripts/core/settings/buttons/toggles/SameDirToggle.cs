using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Settings.Buttons.Directory;
using Godot;

namespace Com.Astral.GodotHub.Settings.Buttons.Toggles
{
	public partial class SameDirToggle : SettingToggle
	{
		[Export] protected DownloadDirButton downloadDirButton;

		protected override void OnToggled(bool pToggled)
		{
			AppConfig.UseInstallDirForDownload = pToggled;
			downloadDirButton.Enabled = !pToggled;
		}

		protected override void Reset()
		{
			button.ButtonPressed = AppConfig.UseInstallDirForDownload;
		}
	}
}
