using Godot;
using SortType = Com.Astral.GodotHub.Tabs.SortedPanel.SortType;


namespace Com.Astral.GodotHub.Tabs.Installs
{
	public partial class InstallsTab : Tab
	{
		[ExportGroup("Panels")]
		[Export] protected SortedPanel installsPanel;
		[Export] protected SortedPanel releasesPanel;
		[Export] protected Button installsButton;
		[Export] protected Button releasesButton;

		[ExportGroup("Sorting")]
		[Export] protected OptionButton sortButton;
		[Export] protected CheckBox orderButton;

		protected SortedPanel currentPanel;

		public override void _Ready()
		{
			sortButton.AddItem(SortType.Version.ToString(), (int)SortType.Version);
			sortButton.AddItem(SortType.Date.ToString(), (int)SortType.Date);
			sortButton.GetPopup().TransparentBg = true;

			installsPanel.Visible = true;
			releasesPanel.Visible = false;
			currentPanel = installsPanel;
			SortCurrent();
		}

		protected override void Connect()
		{
			installsButton.Toggled += OnInstallsToggled;
			releasesButton.Toggled += OnReleasesToggled;
			sortButton.ItemSelected += OnSortChanged;
			orderButton.Pressed += SortCurrent;
		}

		protected override void Disconnect()
		{
			orderButton.Pressed -= SortCurrent;
			sortButton.ItemSelected -= OnSortChanged;
			releasesButton.Toggled -= OnReleasesToggled;
			installsButton.Toggled -= OnInstallsToggled;
		}

		protected void OnInstallsToggled(bool pToggled)
		{
			if (!pToggled || installsPanel.Visible)
				return;

			installsPanel.Visible = true;
			releasesPanel.Visible = false;
			currentPanel = installsPanel;
			SortCurrent();
		}

		protected void OnReleasesToggled(bool pToggled)
		{
			if (!pToggled || releasesPanel.Visible)
				return;

			releasesPanel.Visible = true;
			installsPanel.Visible = false;
			currentPanel = releasesPanel;
			SortCurrent();
		}

		protected void OnSortChanged(long _)
		{
			SortCurrent();
		}

		protected void SortCurrent()
		{
			currentPanel.Sort((SortType)sortButton.Selected, orderButton.ButtonPressed);
		}
	}
}
