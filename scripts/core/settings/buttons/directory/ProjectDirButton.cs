using Com.Astral.GodotHub.Data;

namespace Com.Astral.GodotHub.Settings.Buttons.Directory
{
	public partial class ProjectDirButton : DirButton
	{
		protected override void OnDirSelected(string pDir)
		{
			AppConfig.ProjectDir = pDir;
			base.OnDirSelected(pDir);
		}

		protected override void Reset()
		{
			button.Text = $" {AppConfig.ProjectDir}";
		}
	}
}
