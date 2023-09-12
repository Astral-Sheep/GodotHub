using Godot;

namespace Com.Astral.GodotHub.Tabs.Installs
{
	public partial class InstallsTab : Tab
	{
		[Export] protected CanvasItem installsPanel;
		[Export] protected CanvasItem releasesPanel;
		[Export] protected Button installsButton;
		[Export] protected Button releasesButton;

		public override void _Ready()
		{
			//Uncomment when both panels are done
			//OnInstallsPressed();
		}

		protected void OnInstallsToggled(bool pToggled)
		{
			if (!pToggled || installsPanel.Visible)
				return;

			installsPanel.Visible = true;
			releasesPanel.Visible = false;
		}

		protected void OnReleasesToggled(bool pToggled)
		{
			if (!pToggled || releasesPanel.Visible)
				return;

			releasesPanel.Visible = true;
			installsPanel.Visible = false;
		}
	}
}
