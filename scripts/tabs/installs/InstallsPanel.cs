using Com.Astral.GodotHub.Data;
using Godot;
using System.Collections.Generic;

namespace Com.Astral.GodotHub.Tabs.Installs
{
	public partial class InstallsPanel : SortedPanel
	{
		[Export] protected PackedScene installItemScene;

		protected List<InstallItem> items = new List<InstallItem>();

		public override void _Ready()
		{
			List<Project> lVersions = InstallsData.GetAllVersions();

			for (int i = 0; i < lVersions.Count; i++)
			{
				items.Add(CreateItem(lVersions[i], i));
			}
		}

		protected InstallItem CreateItem(Project pInstall, int pIndex)
		{
			InstallItem lItem = installItemScene.Instantiate<InstallItem>();
			itemContainer.AddChild(lItem);
			lItem.Init(pInstall, pIndex);
			return lItem;
		}

		public override void Sort(SortType pType, bool pReversed)
		{
			SortItems(pType == SortType.Version ? new VersionSorter() : new DateSorter(), pReversed);
		}

		protected void SortItems(IComparer<InstallItem> pComparer, bool pReversed)
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

		protected class DateSorter : IComparer<InstallItem>
		{
			public int Compare(InstallItem x, InstallItem y)
			{
				return x.Index - y.Index;
			}
		}

		protected class VersionSorter : IComparer<InstallItem>
		{
			public int Compare(InstallItem x, InstallItem y)
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
