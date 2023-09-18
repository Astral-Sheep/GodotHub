using System;

namespace Com.Astral.GodotHub.Tabs.Comparisons
{
	public static class Comparer
	{
		public static int CompareFavorites<T>(T pLhs, T pRhs) where T : IFavoriteItem, IValidItem
		{
			return CompareItems(pLhs, pRhs, (lhs, rhs) => rhs.IsFavorite.CompareTo(lhs.IsFavorite));
		}

		public static int CompareMonos<T>(T pLhs, T pRhs) where T : IMonoItem, IValidItem
		{
			return CompareItems(pLhs, pRhs, (lhs, rhs) => rhs.IsMono.CompareTo(lhs.IsMono));
		}

		public static int CompareNames<T>(T pLhs, T pRhs) where T : INamedItem, IValidItem
		{
			return CompareItems(pLhs, pRhs, (lhs, rhs) => lhs.ItemName.CompareTo(rhs.ItemName));
		}

		public static int CompareTimes<T>(T pLhs, T pRhs) where T : ITimedItem, IValidItem
		{
			return CompareItems(pLhs, pRhs, (lhs, rhs) => lhs.TimeSinceLastOpening.CompareTo(rhs.TimeSinceLastOpening));
		}

		public static int CompareVersions<T>(T pLhs, T pRhs) where T : IVersionItem, IValidItem
		{
			return CompareItems(pLhs, pRhs, (lhs, rhs) => rhs.Version.CompareTo(lhs.Version));
		}

		public static int ReversedCompareFavorites<T>(T pLhs, T pRhs) where T : IFavoriteItem, IValidItem
		{
			return CompareFavorites(pRhs, pLhs);
		}

		public static int ReversedCompareMonos<T>(T pLhs, T pRhs) where T : IMonoItem, IValidItem
		{
			return CompareMonos(pRhs, pLhs);
		}

		public static int ReversedCompareNames<T>(T pLhs, T pRhs) where T : INamedItem, IValidItem
		{
			return CompareNames(pRhs, pLhs);
		}

		public static int ReversedCompareTimes<T>(T pLhs, T pRhs) where T : ITimedItem, IValidItem
		{
			return CompareTimes(pRhs, pLhs);
		}

		public static int ReversedCompareVersions<T>(T pLhs, T pRhs) where T : IVersionItem, IValidItem
		{
			return CompareVersions(pRhs, pLhs);
		}

		private static int CompareItems<T>(T pLhs, T pRhs, Func<T, T, int> pComparison) where T : IValidItem
		{
			if (pLhs.IsValid ^ pRhs.IsValid)
			{
				return pLhs.IsValid ? -1 : 1;
			}

			return pComparison(pLhs, pRhs);
		}
	}
}
