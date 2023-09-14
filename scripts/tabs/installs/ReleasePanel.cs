using Com.Astral.GodotHub.Data;
using Godot;
using Octokit;
using System.Collections.Generic;

namespace Com.Astral.GodotHub.Tabs.Installs
{
	public partial class ReleasePanel : SortedPanel
	{
		[Export] protected PackedScene releaseItemScene;

		protected List<ReleaseItem> items = new List<ReleaseItem>();

		public override void _Ready()
		{
			GDRepository.Loaded += OnRepoRetrieved;
		}

		protected void OnRepoRetrieved()
		{
			GDRepository.Loaded -= OnRepoRetrieved;
			List<Release> lReleases = GDRepository.Releases;

			for (int i = 0; i < lReleases.Count; i++)
			{
				items.Add(CreateItem(lReleases[i], i));
			}
		}

		protected ReleaseItem CreateItem(Release pRelease, int pIndex)
		{
			ReleaseItem lItem = releaseItemScene.Instantiate<ReleaseItem>();
			itemContainer.AddChild(lItem);
			lItem.Init(pRelease, pIndex);
			return lItem;
		}

		public override void Sort(SortType pType, bool pReversed)
		{
			SortItems(pType == SortType.Version ? new VersionSorter() : new DateSorter(), pReversed);
		}

		protected void SortItems(IComparer<ReleaseItem> pComparer, bool pReversed)
		{
			items.Sort(pComparer);

			if (pReversed)
			{
				items.Reverse();
			}
			
			for (int i = 0; i < items.Count; i++)
			{
				itemContainer.MoveChild(items[i], i);
			}
		}

		protected class DateSorter : IComparer<ReleaseItem>
		{
			public int Compare(ReleaseItem x, ReleaseItem y)
			{
				return x.Index - y.Index;
			}
		}

		protected class VersionSorter : IComparer<ReleaseItem>
		{
			public int Compare(ReleaseItem x, ReleaseItem y)
			{
				if (x.Version.major == y.Version.major)
				{
					if (x.Version.minor == y.Version.minor)
					{
						return y.Version.patch - x.Version.patch;
					}

					return y.Version.minor - x.Version.minor;
				}

				return y.Version.major - x.Version.major;
			}
		}
	}
}
