using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Utils.Comparisons;
using Godot;
using System;
using System.Collections.Generic;

using Version = Com.Astral.GodotHub.Data.Version;

namespace Com.Astral.GodotHub.Tabs.Installs
{
	public partial class InstallsPanel : SortedPanel
	{
		[Export] protected PackedScene installItemScene;

		[ExportGroup("Install addition")]
		[Export] protected Button addButton;
		[Export] protected PackedScene fileDialogScene;

		[ExportGroup("Sorting")]
		[Export] protected Button favoriteButton;
		[Export] protected SortToggle versionButton;
		[Export] protected SortToggle monoButton;
		[Export] protected SortToggle dateButton;

		protected List<InstallItem> items = new List<InstallItem>();

		public override void _Ready()
		{
			List<GDFile> lVersions = InstallsData.GetAllVersions();

			for (int i = 0; i < lVersions.Count; i++)
			{
				CreateItem(lVersions[i]);
			}

			InstallsData.VersionAdded += OnVersionAdded;
			InstallItem.Closed += OnItemClosed;

			favoriteButton.Toggled += OnFavoriteToggled;
			versionButton.CustomToggled += OnVersionToggled;
			monoButton.CustomToggled += OnMonoToggled;
			dateButton.CustomToggled += OnDateToggled;
			addButton.Pressed += OnAddPressed;

			dateButton.ButtonPressed = true;
		}

		protected override void Dispose(bool pDisposing)
		{
			if (pDisposing)
			{
				InstallItem.Closed -= OnItemClosed;
				InstallsData.VersionAdded -= OnVersionAdded;
			}
		}

		protected InstallItem CreateItem(GDFile pInstall)
		{
			InstallItem lItem = installItemScene.Instantiate<InstallItem>();
			itemContainer.AddChild(lItem);
			lItem.Init(pInstall);
			items.Add(lItem);
			return lItem;
		}

		#region EVENT_HANDLING

		protected void OnVersionAdded(GDFile pInstall)
		{
			CreateItem(pInstall);
		}

		protected void OnItemClosed(InstallItem pItem)
		{
			items.Remove(pItem);
		}

		protected void OnAddPressed()
		{
			FileDialog lDialog = fileDialogScene.Instantiate<FileDialog>();
			Main.Instance.AddChild(lDialog);
			lDialog.PopupCentered();
			lDialog.FileMode = FileDialog.FileModeEnum.OpenFiles;

#if GODOT_WINDOWS
			lDialog.Filters = new string[] { "*.exe" };
#else
			lDialog.Filters = new string [] { "*" };
#endif

			lDialog.CurrentDir = AppConfig.InstallDir;
			lDialog.FilesSelected += OnFilesSelected;
		}

		protected void OnFilesSelected(string[] pPaths)
		{
			string lPath;

			for (int i = 0; i < pPaths.Length; i++)
			{
				lPath = pPaths[i];
				InstallsData.AddVersion(lPath, true);
			}
		}

		protected void OnFavoriteToggled(bool pToggled)
		{
			versionButton.Disable();
			monoButton.Disable();

			if (pToggled)
			{
				dateButton.Disable();
				Sort(Comparer.CompareFavorites);
			}
			else
			{
				dateButton.Enable();
				Sort(Comparer.CompareTimes);
			}
		}

		protected void OnVersionToggled(bool pToggled)
		{
			favoriteButton.SetPressedNoSignal(false);
			monoButton.Disable();
			dateButton.Disable();
			Sort(pToggled ? Comparer.CompareVersions : Comparer.ReversedCompareVersions);
		}

		protected void OnMonoToggled(bool pToggled)
		{
			favoriteButton.SetPressedNoSignal(false);
			versionButton.Disable();
			dateButton.Disable();
			Sort(pToggled ? Comparer.CompareMonos : Comparer.ReversedCompareMonos);
		}

		protected void OnDateToggled(bool pToggled)
		{
			favoriteButton.SetPressedNoSignal(false);
			versionButton.Disable();
			monoButton.Disable();
			Sort(pToggled ? Comparer.CompareTimes : Comparer.ReversedCompareTimes);
		}

		#endregion //EVENT_HANDLING

		protected void Sort(Comparison<InstallItem> pComparison)
		{
			items.Sort(pComparison);

			for (int i = 0; i < items.Count; i++)
			{
				itemContainer.MoveChild(items[i], i);
			}
		}
	}
}
