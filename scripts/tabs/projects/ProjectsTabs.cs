using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Debug;
using Com.Astral.GodotHub.Tabs.Comparisons;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;

using Version = Com.Astral.GodotHub.Data.Version;

namespace Com.Astral.GodotHub.Tabs.Projects
{
	public partial class ProjectsTabs : Tab
	{
		[Export] protected PackedScene projectItemScene;
		[Export] protected Control itemContainer;

		[ExportGroup("Project addition")]
		[Export] protected PackedScene folderPopupScene;
		[Export] protected Button addButton;

		[ExportGroup("Project creation")]
		[Export] protected PackedScene creationPopupScene;
		[Export] protected Button newButton;

		[ExportGroup("Sorting")]
		[Export] protected Button favoriteButton;
		[Export] protected SortToggle nameButton;
		[Export] protected SortToggle dateButton;
		[Export] protected SortToggle versionButton;

		protected List<ProjectItem> items = new List<ProjectItem>();
		protected Comparison<ProjectItem> currentSort;

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
			newButton.Pressed += OnNewPressed;
			addButton.Pressed += OnAddPressed;
			ProjectsData.Added += OnProjectAdded;

			dateButton.ButtonPressed = true;
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposing)
				return;

			ProjectsData.Added -= OnProjectAdded;
		}

		protected ProjectItem CreateItem(GDFile pProject)
		{
			ProjectItem lItem = projectItemScene.Instantiate<ProjectItem>();
			Debugger.PrintMessage(lItem.Name);
			itemContainer.AddChild(lItem);
			lItem.Init(pProject);
			lItem.Closed += OnItemClosed;
			return lItem;
		}

		protected override void Connect() { }

		protected override void Disconnect() { }

		protected void Sort()
		{
			items.Sort(currentSort);

			for (int i = 0; i < items.Count; i++)
			{
				itemContainer.MoveChild(items[i], i);
			}
		}

		#region EVENT_HANDLING

		protected void OnProjectAdded(GDFile pProject)
		{
			CreateItem(pProject);
			Sort();
		}

		protected void OnNewPressed()
		{
			NewProjectDialog lDialog = creationPopupScene.Instantiate<NewProjectDialog>();
			Main.Instance.AddChild(lDialog);
			lDialog.Confirmed += OnProjectConfirmed;
		}

		protected void OnProjectConfirmed(string pName, string pPath, Version pVersion, RenderMode pRenderMode, VersionningMode pVersionningMode)
		{
			ProjectCreator.CreateProject(pName, pPath, pVersion, pRenderMode, pVersionningMode);
		}

		protected void OnAddPressed()
		{
			FileDialog lDialog = Main.Instance.InstantiateFileDialog();
			lDialog.CurrentDir = Config.ProjectDir;
			lDialog.FileMode = FileDialog.FileModeEnum.OpenFiles;
			lDialog.Filters = new string[] { "*.godot" };
			lDialog.FilesSelected += OnFilesSelected;

			//ProjectCreator.AddIcon("C:/Users/thoma/Desktop");
		}

		protected void OnFilesSelected(string[] pPaths)
		{
			string lPath;

			for (int i = 0; i < pPaths.Length; i++)
			{
				lPath = pPaths[i];
				lPath = lPath[..lPath.RFind("/project.godot")];

				if (ProjectsData.HasProject(lPath))
					continue;

				GDFile lProject = new GDFile(lPath, false, ProjectsData.GetVersionFromFolder(lPath));
				ProjectsData.AddProject(lProject);
			}

			Sort();
		}

		protected void OnFavoriteToggled(bool pToggled)
		{
			nameButton.Disable();
			dateButton.Disable();

			if (pToggled)
			{
				versionButton.Disable();
				currentSort = Comparer.CompareFavorites;
			}
			else
			{
				dateButton.Enable();
				currentSort = Comparer.CompareTimes;
			}

			Sort();
		}

		protected void OnNameToggled(bool pToggled)
		{
			favoriteButton.SetPressedNoSignal(false);
			dateButton.Disable();
			versionButton.Disable();

			currentSort = pToggled ? Comparer.CompareNames : Comparer.ReversedCompareNames;
			Sort();
		}

		protected void OnDateToggled(bool pToggled)
		{
			favoriteButton.SetPressedNoSignal(false);
			nameButton.Disable();
			versionButton.Disable();

			currentSort = pToggled ? Comparer.CompareTimes : Comparer.ReversedCompareTimes;
			Sort();
		}

		protected void OnVersionToggled(bool pToggled)
		{
			favoriteButton.SetPressedNoSignal(false);
			nameButton.Disable();
			dateButton.Disable();

			currentSort = pToggled ? Comparer.CompareVersions : Comparer.ReversedCompareVersions;
			Sort();
		}

		protected void OnItemClosed(ProjectItem pItem)
		{
			items.Remove(pItem);
		}

		#endregion //EVENT_HANDLING
	}
}
