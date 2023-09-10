using Godot;
using Octokit;
using System.Collections.Generic;

namespace Com.Astral.GodotHub.Releases
{
	public partial class ReleasePanel : Control
	{
		protected enum SortType
		{
			Date = 0,
			Version = 1,
		}

		[Export] protected OptionButton sortButton;
		[Export] protected CheckBox orderButton;
		[ExportGroup("Release item")]
		[Export] protected PackedScene releaseItemScene;
		[Export] protected Control itemContainer;
		protected List<ReleaseItem> items = new List<ReleaseItem>();

		public override void _Ready()
		{
			sortButton.AddItem(SortType.Date.ToString(), (int)SortType.Date);
			sortButton.AddItem(SortType.Version.ToString(), (int)SortType.Version);
			sortButton.GetPopup().TransparentBg = true;
			GodotRepo.RepoRetrieved += OnRepoRetrieved;
		}

		protected void OnRepoRetrieved()
		{
			GodotRepo.RepoRetrieved -= OnRepoRetrieved;
			IReadOnlyList<Release> lReleases = GodotRepo.GetReleases();

			for (int i = 0; i < lReleases.Count; i++)
			{
				items.Add(CreateItem(lReleases[i], i));
			}

			Sort();
			sortButton.ItemSelected += OnSortChanged;
			orderButton.Toggled += OnOrderChanged;
		}

		protected ReleaseItem CreateItem(Release pRelease, int pIndex)
		{
			ReleaseItem lItem = releaseItemScene.Instantiate<ReleaseItem>();
			itemContainer.AddChild(lItem);
			lItem.Init(pRelease, pIndex);
			return lItem;
		}

		protected void OnSortChanged(long _)
		{
			Sort();
		}

		protected void OnOrderChanged(bool _)
		{
			Sort();
		}

		protected void Sort()
		{
			if (sortButton.Selected == (long)SortType.Date)
			{
				SortLatest();
			}
			else
			{
				SortVersion();
			}
		}

		protected void SortLatest()
		{
			SortItems(new DateSorter(), orderButton.ButtonPressed);
		}

		protected void SortVersion()
		{
			SortItems(new VersionSorter(), orderButton.ButtonPressed);
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
