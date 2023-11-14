using Com.Astral.GodotHub.Core.Data;
using Com.Astral.GodotHub.Core.Utils.Comparisons;
using Godot;
using Octokit;
using System;
using System.Collections.Generic;

using Version = Com.Astral.GodotHub.Core.Data.Version;

namespace Com.Astral.GodotHub.Core.Tabs.Installs
{
	public partial class ReleasePanel : SortedPanel
	{
		[Export] protected PackedScene releaseItemScene;

		[ExportGroup("Sorting")]
		[Export] protected SortToggle versionButton;
		[Export] protected SortToggle dateButton;

		protected List<ReleaseItem> items = new List<ReleaseItem>();

		public override void _Ready()
		{
			GDRepository.Loaded += OnRepoRetrieved;
		}

		protected void OnRepoRetrieved()
		{
			GDRepository.Loaded -= OnRepoRetrieved;
			List<Release> lReleases = GDRepository.Releases;
			Release lRelease;

			for (int i = 0; i < lReleases.Count; i++)
			{
				lRelease = lReleases[i];

				if ((Version)lRelease.Name < Version.minimumSupportedVersion)
					continue;

				items.Add(CreateItem(lReleases[i], i));
			}

			versionButton.CustomToggled += OnVersionToggled;
			dateButton.CustomToggled += OnDateToggled;
			dateButton.ButtonPressed = true;
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
			Sort(pToggled ? Comparer.CompareVersions : Comparer.ReversedCompareVersions);
		}

		protected void OnDateToggled(bool pToggled)
		{
			versionButton.Disable();
			Sort(pToggled ? CompareIndices : ReversedCompareIndices);
		}

		protected void Sort(Comparison<ReleaseItem> pComparison)
		{
			items.Sort(pComparison);

			for (int i = 0; i < items.Count; i++)
			{
				itemContainer.MoveChild(items[i], i);
			}
		}

		protected int CompareIndices(ReleaseItem pLhs, ReleaseItem pRhs)
		{
			return pLhs.Index - pRhs.Index;
		}

		protected int ReversedCompareIndices(ReleaseItem pLhs, ReleaseItem pRhs)
		{
			return CompareIndices(pRhs, pLhs);
		}
	}
}
