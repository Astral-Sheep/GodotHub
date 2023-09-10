namespace Com.Astral.GodotHub.Settings.Buttons
{
	public partial class ProjectDirButton : DirButton
	{
		protected override void OnDirSelected(string pDir)
		{
			Config.ProjectDir = pDir;
			base.OnDirSelected(pDir);
		}

		protected override void Reset()
		{
			button.Text = Config.ProjectDir;
		}
	}
}
