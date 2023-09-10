namespace Com.Astral.GodotHub.Settings.Buttons
{
	public partial class ProjectDirButton : DirButton
	{
		public override void _Ready()
		{
			button.Text = Config.ProjectDir;
		}

		protected override void OnDirSelected(string pDir)
		{
			Config.ProjectDir = pDir;
			base.OnDirSelected(pDir);
		}
	}
}
