using System;

namespace Com.Astral.GodotHub.Core.Utils
{
	public static class TimeFormater
	{
		public static string Format(DateTime pTime)
		{
			DateTime lCurrentTime = DateTime.UtcNow;
			TimeSpan lDifferenceSpan = lCurrentTime - pTime;

			//To refactor: I don't think it needs an explanation of why
			if (pTime.Year < lCurrentTime.Year)
			{
				int lDifference = lCurrentTime.Year - pTime.Year;

				if (lDifference == 1 && lCurrentTime.Month < pTime.Month)
				{
					return FormatInternal(pTime.Month - lCurrentTime.Month, "month");
				}
				else
				{
					return FormatInternal(lDifference, "year");
				}
			}
			else if (pTime.Month < lCurrentTime.Month)
			{
				int lDifference = lCurrentTime.Month - pTime.Month;

				if (lDifference == 1 && lCurrentTime.Day < pTime.Day)
				{
					return FormatInternal(lDifferenceSpan.Days, "day");
				}
				else
				{
					return FormatInternal(lDifference, "month");
				}
			}
			else if (pTime.Day < lCurrentTime.Day)
			{
				if (lDifferenceSpan.TotalHours < 24)
				{
					return FormatInternal(lDifferenceSpan.Hours, "hour");
				}
				else
				{
					return FormatInternal(lDifferenceSpan.Days, "day");
				}
			}
			else if (pTime.Hour < lCurrentTime.Hour)
			{
				if (lDifferenceSpan.TotalMinutes < 60)
				{
					return FormatInternal(lDifferenceSpan.Minutes, "minute");
				}
				else
				{
					return FormatInternal(lDifferenceSpan.Hours, "hour");
				}
			}
			else if (pTime.Minute < lCurrentTime.Minute)
			{
				if (lDifferenceSpan.TotalSeconds < 60)
				{
					return FormatInternal(lDifferenceSpan.Seconds, "second");
				}
				else
				{
					return FormatInternal(lDifferenceSpan.Minutes, "minute");
				}
			}
			else if (pTime.Second < lCurrentTime.Second)
			{
				if (lDifferenceSpan.TotalSeconds < 1)
				{
					return "Now";
				}
				else
				{
					return FormatInternal(lDifferenceSpan.Seconds, "second");

				}
			}
			else
			{
				return "Now";
			}
		}

		private static string FormatInternal(int pValue, string pSuffix)
		{
			return $"{pValue} {pSuffix}{(pValue > 1 ? "s" : "")} ago";
		}
	}
}
