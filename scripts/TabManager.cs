using Godot;

namespace Com.Astral.GodotHub
{
	public partial class TabManager : Node
	{
		[ExportGroup("Tabs")]
		[Export] protected Tab projectsTab;
		[Export] protected Tab versionsTab;
		[Export] protected Tab documentationTab;

		[ExportGroup("Buttons")]
		[Export] protected Button projectsButton;
		[Export] protected Button versionsButton;
		[Export] protected Button documentationButton;

		public override void _Ready()
		{
			projectsButton.Toggled += OnProjectsToggled;
			versionsButton.Toggled += OnVersionsToggled;
			documentationButton.Toggled += OnDocumentationToggled;
			projectsTab.Visible = projectsButton.ButtonPressed;
			versionsTab.Visible = versionsButton.ButtonPressed;
			documentationTab.Visible = documentationButton.ButtonPressed;
		}

		protected void OnProjectsToggled(bool pToggle)
		{
			if (projectsTab.Visible || !pToggle)
				return;

			projectsTab.Visible = true;
			versionsTab.Visible = false;
			documentationTab.Visible = false;
		}

		protected void OnVersionsToggled(bool pToggle)
		{
			if (versionsTab.Visible || !pToggle)
				return;
			
			versionsTab.Visible = true;
			projectsTab.Visible = false;
			documentationTab.Visible = false;
		}

		protected void OnDocumentationToggled(bool pToggle)
		{
			if (documentationTab.Visible || !pToggle)
				return;

			documentationTab.Visible = true;
			projectsTab.Visible = false;
			versionsTab.Visible = false;
		}
	}
}
