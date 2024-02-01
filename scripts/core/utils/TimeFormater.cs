using System;

namespace Com.Astral.GodotHub.Core.Utils
{
	public static class TimeFormater
	{
		private const string YEAR = "year";
		private const string MONTH = "month";
		private const string DAY = "day";
		private const string HOUR = "hour";
		private const string MINUTE = "minute";
		private const string SECOND = "second";
		private const string NOW = "Now";
		
		public static string Format(DateTime pTime)
		{
			DateTime lCurrentTime = DateTime.UtcNow;
			TimeSpan lDifferenceSpan = lCurrentTime - pTime;

			// Basic comparisons
			if (lDifferenceSpan.TotalSeconds < 1d)
			{
				return NOW;
			}

			if (lDifferenceSpan.TotalMinutes < 1d)
			{
				return FormatInternal((int)Math.Floor(lDifferenceSpan.TotalSeconds), SECOND);
			}

			if (lDifferenceSpan.TotalHours < 1d)
			{
				return FormatInternal((int)Math.Floor(lDifferenceSpan.TotalMinutes), MINUTE);
			}

			if (lDifferenceSpan.TotalDays < 1d)
			{
				return FormatInternal((int)Math.Floor(lDifferenceSpan.TotalHours), HOUR);
			}

			// Comparisons that need more information on the date of the year
			int lYearDiff = (int)Math.Floor(lDifferenceSpan.TotalDays / 365.2425);

			if (lYearDiff > 0)
			{
				return FormatInternal(lYearDiff, YEAR);
			}
			
			if (lCurrentTime.Month == pTime.Month)
			{
				return FormatInternal((int)Math.Floor(lDifferenceSpan.TotalDays), DAY);
			}
			
			int lMonthDiff = lCurrentTime.Month - pTime.Month;

			if (lMonthDiff < 0)
			{
				lMonthDiff += 12;
			}

			if (lMonthDiff == 1 && lCurrentTime.Day < pTime.Day)
			{
				return FormatInternal((int)Math.Floor(lDifferenceSpan.TotalDays), DAY);
			}

			return FormatInternal(lMonthDiff, MONTH);
		}
		
		private static string FormatInternal(int pValue, string pSuffix)
		{
			// Will need refactoring if localisation is added
			return $"{pValue} {pSuffix}{(pValue > 1 ? "s" : "")} ago";
		}
	}
}
