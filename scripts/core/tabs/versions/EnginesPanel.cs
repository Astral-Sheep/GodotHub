using Com.Astral.GodotHub.Core.Data;
using Com.Astral.GodotHub.Core.Utils.Comparisons;
using Godot;
using System;
using System.Collections.Generic;

namespace Com.Astral.GodotHub.Core.Tabs.Versions
{
	public partial class EnginesPanel : SortedPanel
	{
		[Export] protected PackedScene engineItemScene;

		[ExportGroup("Engine addition")]
		[Export] protected Button addButton;
		[Export] protected PackedScene fileDialogScene;

		[ExportGroup("Sorting")]
		[Export] protected Button favoriteButton;
		[Export] protected SortToggle versionButton;
		[Export] protected SortToggle monoButton;
		[Export] protected SortToggle dateButton;

		protected List<EngineItem> items = new List<EngineItem>();
		protected Comparison<EngineItem> currentComparison = Comparer.CompareTimes;

		public override void _Ready()
		{
			List<GDFile> lVersions = VersionsData.GetAllVersions();

			for (int i = 0; i < lVersions.Count; i++)
			{
				CreateItem(lVersions[i]);
			}

			Sort();

			VersionsData.VersionAdded += OnVersionAdded;
			EngineItem.Closed += OnItemClosed;

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
				EngineItem.Closed -= OnItemClosed;
				VersionsData.VersionAdded -= OnVersionAdded;
			}
		}

		protected EngineItem CreateItem(GDFile pInstall)
		{
			EngineItem lItem = engineItemScene.Instantiate<EngineItem>();
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

		protected void OnItemClosed(EngineItem pItem)
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
				VersionsData.AddVersion(lPath, true);
			}
		}

		protected void OnFavoriteToggled(bool pToggled)
		{
			versionButton.Disable();
			monoButton.Disable();

			if (pToggled)
			{
				dateButton.Disable();
				currentComparison = Comparer.CompareFavorites;
				Sort();
			}
			else
			{
				dateButton.Enable();
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
			Sort();
		}

		protected void OnMonoToggled(bool pToggled)
		{
			favoriteButton.SetPressedNoSignal(false);
			versionButton.Disable();
			dateButton.Disable();
			currentComparison = pToggled ? Comparer.CompareMonos : Comparer.ReversedCompareMonos;
			Sort();
		}

		protected void OnDateToggled(bool pToggled)
		{
			favoriteButton.SetPressedNoSignal(false);
			versionButton.Disable();
			monoButton.Disable();
			currentComparison = pToggled ? Comparer.CompareTimes : Comparer.ReversedCompareTimes;
			Sort();
		}

		#endregion //EVENT_HANDLING

		protected void Sort()
		{
			items.Sort(currentComparison);

			for (int i = 0; i < items.Count; i++)
			{
				itemContainer.MoveChild(items[i], i);
			}
		}
	}
}
