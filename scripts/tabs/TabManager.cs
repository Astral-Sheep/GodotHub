using Godot;

namespace Com.Astral.GodotHub.Tabs
{
	public partial class TabManager : Control
	{
		[ExportGroup("Tabs")]
		[Export] protected Tab projectsTab;
		[Export] protected Tab installsTab;
		[Export] protected Tab documentationTab;

		[ExportGroup("Buttons")]
		[Export] protected Button projectsButton;
		[Export] protected Button installsButton;
		[Export] protected Button documentationButton;

		public override void _Ready()
		{
			projectsButton.Toggled += OnProjectsToggled;
			installsButton.Toggled += OnInstallsToggled;
			documentationButton.Toggled += OnDocumentationToggled;

			projectsTab.Visible = projectsButton.ButtonPressed;
			installsTab.Visible = installsButton.ButtonPressed;
			documentationTab.Visible = documentationButton.ButtonPressed;
		}

		protected void OnProjectsToggled(bool pToggle)
		{
			if (projectsTab.Visible || !pToggle)
				return;

			projectsTab.Visible = true;
			installsTab.Visible = false;
			documentationTab.Visible = false;
		}

		protected void OnInstallsToggled(bool pToggle)
		{
			if (installsTab.Visible || !pToggle)
				return;
			
			installsTab.Visible = true;
			projectsTab.Visible = false;
			documentationTab.Visible = false;
		}

		protected void OnDocumentationToggled(bool pToggle)
		{
			if (documentationTab.Visible || !pToggle)
				return;

			documentationTab.Visible = true;
			projectsTab.Visible = false;
			installsTab.Visible = false;
		}
	}
}
