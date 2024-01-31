using Com.Astral.GodotHub.Core.Data;
using Com.Astral.GodotHub.Core.Utils.Comparisons;
using Godot;
using System;
using System.Collections.Generic;

using Version = Com.Astral.GodotHub.Core.Data.Version;

namespace Com.Astral.GodotHub.Core.Tabs.Installs
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
		protected Comparison<InstallItem> currentComparison = Comparer.CompareTimes;

		public override void _Ready()
		{
			List<GDFile> lVersions = InstallsData.GetAllVersions();

			for (int i = 0; i < lVersions.Count; i++)
			{
				CreateItem(lVersions[i]);
			}

			Sort();

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
			Sort();
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
				// Sort(Comparer.CompareFavorites);
				currentComparison = Comparer.CompareFavorites;
				Sort();
			}
			else
			{
				dateButton.Enable();
				// Sort(Comparer.CompareTimes);
				currentComparison = Comparer.CompareTimes;
				Sort();
			}
		}

		protected void OnVersionToggled(bool pToggled)
		{
			favoriteButton.SetPressedNoSignal(false);
			monoButton.Disable();
			dateButton.Disable();
			currentComparison = pToggled ? Comparer.CompareVersions : Comparer.ReversedCompareVersions;
			// Sort(pToggled ? Comparer.CompareVersions : Comparer.ReversedCompareVersions);
			Sort();
		}

		protected void OnMonoToggled(bool pToggled)
		{
			favoriteButton.SetPressedNoSignal(false);
			versionButton.Disable();
			dateButton.Disable();
			currentComparison = pToggled ? Comparer.CompareMonos : Comparer.ReversedCompareMonos;
			// Sort(pToggled ? Comparer.CompareMonos : Comparer.ReversedCompareMonos);
			Sort();
		}

		protected void OnDateToggled(bool pToggled)
		{
			favoriteButton.SetPressedNoSignal(false);
			versionButton.Disable();
			monoButton.Disable();
			currentComparison = pToggled ? Comparer.CompareTimes : Comparer.ReversedCompareTimes;
			Sort(/*pToggled ? Comparer.CompareTimes : Comparer.ReversedCompareTimes*/);
		}

		#endregion //EVENT_HANDLING

		protected void Sort(/*Comparison<InstallItem> pComparison*/)
		{
			items.Sort(currentComparison);

			for (int i = 0; i < items.Count; i++)
			{
				itemContainer.MoveChild(items[i], i);
			}
		}
	}
}
