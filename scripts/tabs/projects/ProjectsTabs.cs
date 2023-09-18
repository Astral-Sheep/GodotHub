using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Tabs.Comparisons;
using Godot;
using System;
using System.Collections.Generic;

namespace Com.Astral.GodotHub.Tabs.Projects
{
	public partial class ProjectsTabs : Tab
	{
		[Export] protected PackedScene projectItemScene;
		[Export] protected Control itemContainer;

		[ExportGroup("Project addition")]
		[Export] protected PackedScene folderPopupScene;
		[Export] protected Button addButton;

		[ExportGroup("Sorting")]
		[Export] protected Button favoriteButton;
		[Export] protected SortToggle nameButton;
		[Export] protected SortToggle dateButton;
		[Export] protected SortToggle versionButton;

		protected List<ProjectItem> items = new List<ProjectItem>();

		public override void _Ready()
		{
			List<GDFile> lProjects = ProjectsData.GetProjects();
			ProjectsData.GetVersionFromFolder(lProjects[1].Path);

			for (int i = 0; i < lProjects.Count; i++)
			{
				items.Add(CreateItem(lProjects[i]));
			}

			favoriteButton.Toggled += OnFavoriteToggled;
			nameButton.CustomToggled += OnNameToggled;
			dateButton.CustomToggled += OnDateToggled;
			versionButton.CustomToggled += OnVersionToggled;
			addButton.Pressed += OnAddPressed;

			dateButton.ButtonPressed = true;
		}

		protected ProjectItem CreateItem(GDFile pProject)
		{
			ProjectItem lItem = projectItemScene.Instantiate<ProjectItem>();
			itemContainer.AddChild(lItem);
			lItem.Init(pProject);
			return lItem;
		}

		protected override void Connect() { }

		protected override void Disconnect() { }

		protected void Sort(Comparison<ProjectItem> pComparison)
		{
			items.Sort(pComparison);

			for (int i = 0; i < items.Count; i++)
			{
				itemContainer.MoveChild(items[i], i);
			}
		}

		#region EVENT_HANDLING

		protected void OnAddPressed()
		{
			FileDialog lDialog = folderPopupScene.Instantiate<FileDialog>();
			Main.Instance.AddChild(lDialog);
			lDialog.PopupCentered();
			lDialog.CurrentDir = Config.ProjectDir;
			lDialog.FileMode = FileDialog.FileModeEnum.OpenFiles;
			lDialog.Filters = new string[] { "*.godot" };
			lDialog.FileSelected += OnFileSelected;
			lDialog.FilesSelected += OnFilesSelected;
		}

		protected void OnFileSelected(string pPath)
		{
			pPath = pPath[..pPath.RFind("/project.godot")];

			if (ProjectsData.HasProject(pPath))
				return;

			GDFile lProject = new GDFile(pPath, false, ProjectsData.GetVersionFromFolder(pPath));
			ProjectsData.AddProject(lProject);
			CreateItem(lProject);
		}

		protected void OnFilesSelected(string[] pPaths)
		{
			for (int i = 0; i < pPaths.Length; i++)
			{
				OnFileSelected(pPaths[i]);
			}
		}

		protected void OnFavoriteToggled(bool pToggled)
		{
			nameButton.Disable();
			dateButton.Disable();

			if (pToggled)
			{
				versionButton.Disable();
				Sort(Comparer.CompareFavorites);
			}
			else
			{
				dateButton.Enable();
				Sort(Comparer.CompareTimes);
			}
		}

		protected void OnNameToggled(bool pToggled)
		{
			favoriteButton.SetPressedNoSignal(false);
			dateButton.Disable();
			versionButton.Disable();

			Sort(pToggled ? Comparer.CompareNames : Comparer.ReversedCompareNames);
		}

		protected void OnDateToggled(bool pToggled)
		{
			favoriteButton.SetPressedNoSignal(false);
			nameButton.Disable();
			versionButton.Disable();

			Sort(pToggled ? Comparer.CompareTimes : Comparer.ReversedCompareTimes);
		}

		protected void OnVersionToggled(bool pToggled)
		{
			favoriteButton.SetPressedNoSignal(false);
			nameButton.Disable();
			dateButton.Disable();

			Sort(pToggled ? Comparer.CompareVersions : Comparer.ReversedCompareVersions);
		}

		#endregion //EVENT_HANDLING
	}
}
