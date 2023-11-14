using Com.Astral.GodotHub.Core.Data;
using Godot;

namespace Com.Astral.GodotHub.Core.Settings.Buttons.Directory
{
	public partial class InstallDirButton : DirButton
	{
		[Export] protected DownloadDirButton downloadDirButton;

		protected override void OnDirSelected(string pDir)
		{
			AppConfig.InstallDir = pDir;

			if (AppConfig.UseInstallDirForDownload)
			{
				AppConfig.DownloadDir = pDir;
				downloadDirButton.Text = $" {pDir}";
			}

			base.OnDirSelected(pDir);
		}

		protected override void Reset()
		{
			button.Text = $" {AppConfig.InstallDir}";
		}
	}
}
