using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Debug;
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
		[Export] protected SortToggle favoriteButton;
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

			favoriteButton.CustomToggled += OnFavoriteToggled;
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
			versionButton.Disable();

			Sort(pToggled ? CompareFavorites : ReversedCompareFavorites);
		}

		protected void OnNameToggled(bool pToggled)
		{
			favoriteButton.Disable();
			dateButton.Disable();
			versionButton.Disable();

			Sort(pToggled ? CompareNames : ReversedCompareNames);
		}

		protected void OnDateToggled(bool pToggled)
		{
			favoriteButton.Disable();
			nameButton.Disable();
			versionButton.Disable();

			Sort(pToggled ? CompareDates : ReversedCompareDates);
		}

		protected void OnVersionToggled(bool pToggled)
		{
			favoriteButton.Disable();
			nameButton.Disable();
			dateButton.Disable();

			Sort(pToggled ? CompareVersions : ReversedCompareVersions);
		}

		#endregion //EVENT_HANDLING

		#region COMPARISONS

		protected int CompareFavorites(ProjectItem pLhs, ProjectItem pRhs)
		{
			return CompareItems(pLhs, pRhs, (lhs, rhs) => {
				if (lhs.IsFavorite ^ rhs.IsFavorite)
				{
					return lhs.IsFavorite ? -1 : 1;
				}

				return CompareDates(lhs, rhs);
			});
		}

		protected int ReversedCompareFavorites(ProjectItem pLhs, ProjectItem pRhs)
		{
			return CompareFavorites(pRhs, pLhs);
		}

		protected int CompareNames(ProjectItem pLhs, ProjectItem pRhs)
		{
			return CompareItems(pLhs, pRhs, (lhs, rhs) => lhs.ProjectName.CompareTo(rhs.ProjectName));
		}

		protected int ReversedCompareNames(ProjectItem pLhs, ProjectItem pRhs)
		{
			return CompareNames(pRhs, pLhs);
		}

		protected int CompareDates(ProjectItem pLhs, ProjectItem pRhs)
		{
			return CompareItems(pLhs, pRhs, (lhs, rhs) => lhs.TimeSinceLastOpening.CompareTo(rhs.TimeSinceLastOpening));
		}

		protected int ReversedCompareDates(ProjectItem pLhs, ProjectItem pRhs)
		{
			return CompareDates(pRhs, pLhs);
		}

		protected int CompareVersions(ProjectItem pLhs, ProjectItem pRhs)
		{
			return CompareItems(pLhs, pRhs, (lhs, rhs) => rhs.Version.CompareTo(lhs.Version));
		}

		protected int ReversedCompareVersions(ProjectItem pLhs, ProjectItem pRhs)
		{
			return CompareVersions(pRhs, pLhs);
		}

		protected int CompareItems(ProjectItem pLhs, ProjectItem pRhs, Func<ProjectItem, ProjectItem, int> pComparison)
		{
			if (pLhs.IsValid ^ pRhs.IsValid)
			{
				return pLhs.IsValid ? -1 : 1;
			}

			return pComparison(pLhs, pRhs);
		}

		#endregion //COMPARISONS
	}
}
