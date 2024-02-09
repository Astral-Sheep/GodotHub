using Com.Astral.GodotHub.Core.Data;
using Com.Astral.GodotHub.Core.Utils.Comparisons;
using Godot;
using Octokit;
using System;
using System.Collections.Generic;

using Version = Com.Astral.GodotHub.Core.Data.Version;

namespace Com.Astral.GodotHub.Core.Tabs.Versions
{
	public partial class ReleasePanel : SortedPanel
	{
		[Export] protected PackedScene releaseItemScene;

		[ExportGroup("Sorting")]
		[Export] protected SortToggle versionButton;
		[Export] protected SortToggle dateButton;

		protected List<ReleaseItem> items = new List<ReleaseItem>();
		protected Comparison<ReleaseItem> currentComparison = CompareIndices;

		public override void _Ready()
		{
			GDRepository.Loaded += OnRepoLoaded;
		}

		protected override void Dispose(bool pDisposing)
		{
			base.Dispose(pDisposing);

			if (!pDisposing)
				return;

			GDRepository.Loaded -= OnRepoLoaded;
			GDRepository.Updated -= OnRepoUpdated;
		}

		protected void OnRepoLoaded()
		{
			GDRepository.Loaded -= OnRepoLoaded;
			List<Release> lReleases = GDRepository.Releases;
			Release lRelease;

			for (int i = 0; i < lReleases.Count; i++)
			{
				lRelease = lReleases[i];

				if ((Version)lRelease.TagName < Version.minimumSupportedVersion)
					continue;

				items.Add(CreateItem(lReleases[i], i));
			}
			
			Sort();

			versionButton.CustomToggled += OnVersionToggled;
			dateButton.CustomToggled += OnDateToggled;
			dateButton.ButtonPressed = true;

			GDRepository.Updated += OnRepoUpdated;
		}

		protected void OnRepoUpdated(List<Release> pReleases)
		{
			if (pReleases.Count <= 0)
				return;
			
			for (int i = 0; i < pReleases.Count; i++)
			{
				if ((Version)pReleases[i].TagName < Version.minimumSupportedVersion)
					continue;
				
				items.Add(CreateItem(pReleases[i], i));
			}
			
			Sort();
		}

		protected ReleaseItem CreateItem(Release pRelease, int pIndex)
		{
			ReleaseItem lItem = releaseItemScene.Instantiate<ReleaseItem>();
			itemContainer.AddChild(lItem);
			lItem.Init(pRelease, pIndex);
			return lItem;
		}

		protected void OnVersionToggled(bool pToggled)
		{
			dateButton.Disable();
			currentComparison = pToggled ? Comparer.CompareVersions : Comparer.ReversedCompareVersions;
			Sort();
		}

		protected void OnDateToggled(bool pToggled)
		{
			versionButton.Disable();
			currentComparison = pToggled ? CompareIndices : ReversedCompareIndices;
			Sort();
		}

		protected void Sort()
		{
			items.Sort(currentComparison);

			for (int i = 0; i < items.Count; i++)
			{
				itemContainer.MoveChild(items[i], i);
			}
		}

		protected static int CompareIndices(ReleaseItem pLhs, ReleaseItem pRhs)
		{
			return pLhs.Index - pRhs.Index;
		}

		protected static int ReversedCompareIndices(ReleaseItem pLhs, ReleaseItem pRhs)
		{
			return CompareIndices(pRhs, pLhs);
		}
	}
}
