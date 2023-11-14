using Com.Astral.GodotHub.Core.Debug;
using System;
using System.Text.RegularExpressions;

namespace Com.Astral.GodotHub.Core.Data
{
	/// <summary>
	/// Representation of a Godot version as major.minor.patch
	/// </summary>
	public struct Version : IComparable<Version>
	{
		/// <summary>
		/// <see cref="Version"/>s older than this one aren't supported by Godot Hub
		/// since it would be too much work for little to no use
		/// </summary>
		public static readonly Version minimumSupportedVersion = new Version(3, 1, 1);
		private static readonly Regex expression = new Regex(@"([0-9]+)([.]{1}?[0-9]+){1,2}");

		public int major;
		public int minor;
		public int patch;

		public Version(int pMajor, int pMinor, int pPatch)
		{
			major = pMajor;
			minor = pMinor;
			patch = pPatch;
		}

		/// <summary>
		/// For 4.x: whether or not each <see cref="Version"/> has the same <see cref="major"/> and <see cref="minor"/> indices<br/>
		/// For 3.x: whether or not each <see cref="Version"/> has the same <see cref="major"/> index
		/// </summary>
		public bool IsCompatible(Version pOther)
		{
			if (major >= 4)
			{
				return major == pOther.major && minor == pOther.minor;
			}
			else
			{
				return major == pOther.major;
			}
		}

		public int CompareTo(Version pOther)
		{
			if (major == pOther.major)
			{
				if (minor == pOther.minor)
				{
					return patch - pOther.patch;
				}

				return minor - pOther.minor;
			}

			return major - pOther.major;
		}

		public override bool Equals(object pObj)
		{
			if (pObj is Version version)
			{
				return this == version;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return major ^ minor ^ patch;
		}

		public override string ToString()
		{
			return (string)this;
		}

		public static bool operator==(Version pLhs, Version pRhs)
		{
			return pLhs.major == pRhs.major && pLhs.minor == pRhs.minor && pLhs.patch == pRhs.patch;
		}

		public static bool operator!=(Version pLhs, Version pRhs)
		{
			return !(pLhs == pRhs);
		}

		public static bool operator <(Version pLhs, Version pRhs)
		{
			return pLhs.CompareTo(pRhs) < 0;
		}

		public static bool operator <=(Version pLhs, Version pRhs)
		{
			return pLhs.CompareTo(pRhs) <= 0;
		}

		public static bool operator >(Version pLhs, Version pRhs)
		{
			return pLhs.CompareTo(pRhs) > 0;
		}

		public static bool operator >=(Version pLhs, Version pRhs)
		{
			return pLhs.CompareTo(pRhs) >= 0;
		}

		public static explicit operator Version(string pValue)
		{
			Match lMatch = expression.Match(pValue);

			if (!lMatch.Success)
			{
				Debugger.LogError($"Invalid format: can't convert string \"{pValue}\" to {nameof(Version)}");
				return new Version(0, 0, 0);
			}

			CaptureCollection lMinorCapture = lMatch.Groups[2].Captures;
			return new Version(
				int.Parse(lMatch.Groups[1].Value),
				int.Parse(lMinorCapture[0].Value[1..]),
				lMinorCapture.Count > 1 ? int.Parse(lMinorCapture[1].Value[1..]) : 0
			);
		}

		public static explicit operator string(Version pValue)
		{
			string lResult = $"{pValue.major}.{pValue.minor}";

			if (pValue.patch != 0)
			{
				lResult += $".{pValue.patch}";
			}

			return lResult;
		}
	}
}
