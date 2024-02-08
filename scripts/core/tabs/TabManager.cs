using Godot;

namespace Com.Astral.GodotHub.Core.Tabs
{
	public partial class TabManager : Control
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
#if DEBUG
			documentationButton.Toggled += OnDocumentationToggled;
#endif //DEBUG

			projectsTab.Toggle(projectsButton.ButtonPressed);
			versionsTab.Toggle(versionsButton.ButtonPressed);
#if DEBUG
			documentationTab.Toggle(documentationButton.ButtonPressed);
#else
			documentationButton.Visible = false;
			documentationTab.Visible = false;
#endif //DEBUG
		}

		protected void OnProjectsToggled(bool pToggle)
		{
			if (projectsTab.Visible || !pToggle)
				return;

			projectsTab.Toggle(true);
			versionsTab.Toggle(false);
#if DEBUG
			documentationTab.Toggle(false);
#endif //DEBUG
		}

		protected void OnVersionsToggled(bool pToggle)
		{
			if (versionsTab.Visible || !pToggle)
				return;
			
			versionsTab.Toggle(true);
			projectsTab.Toggle(false);
#if DEBUG
			documentationTab.Toggle(false);
#endif //DEBUG
		}

#if DEBUG
		protected void OnDocumentationToggled(bool pToggle)
		{
			if (documentationTab.Visible || !pToggle)
				return;

			documentationTab.Toggle(true);
			projectsTab.Toggle(false);
			versionsTab.Toggle(false);
		}
#endif //DEBUG
	}
}
