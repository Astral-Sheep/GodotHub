using Octokit;

namespace Com.Astral.GodotHub.Data
{
	/// <summary>
	/// Struct representing a <see cref="ReleaseAsset"/> with additional data
	/// </summary>
	public class Source
	{
		/// <summary>
		/// The github release
		/// </summary>
		public ReleaseAsset asset;
		/// <summary>
		/// The <see cref="Version"/> of the release
		/// </summary>
		public Version version;
		/// <summary>
		/// Whether or not it has C# support
		/// </summary>
		public bool mono;
		/// <summary>
		/// The target <see cref="OS"/> of the release
		/// </summary>
		public OS os;
		/// <summary>
		/// The target <see cref="Architecture"/> of the release
		/// </summary>
		public Architecture? architecture;

		public Source() { }

		public Source(ReleaseAsset pAsset, Version pVersion, bool pMono, OS pOS, Architecture? pArchitecture = null)
		{
			asset = pAsset;
			version = pVersion;
			mono = pMono;
			os = pOS;
			architecture = pArchitecture;
		}

		/// <summary>
		/// Return version of the given release
		/// </summary>
		public static Version GetVersion(Release pRelease)
		{
			return (Version)pRelease.Name;
		}

		/// <summary>
		/// Whether or not the given <see cref="Release"/> has C# support
		/// </summary>
		public static bool IsMono(ReleaseAsset pAsset)
		{
			return pAsset.Name.Contains("mono");
		}

		/// <summary>
		/// Return the target <see cref="OS"/> of the given <see cref="ReleaseAsset"/>
		/// </summary>
		public static OS GetOS(ReleaseAsset pAsset)
		{
			if (pAsset.Name.Contains("win"))
			{
				return OS.Windows;
			}
			else if (pAsset.Name.Contains("linux"))
			{
				return OS.Linux;
			}
			else
			{
				return OS.MacOS;
			}
		}

		/// <summary>
		/// Return the target <see cref="Architecture"/> of the <see cref="ReleaseAsset"/>
		/// </summary>
		public static Architecture GetArchitecture(ReleaseAsset pAsset)
		{
			if (pAsset.Name.Contains("64"))
			{
				return Architecture.x64;
			}
			else
			{
				return Architecture.x32;
			}
		}
	}
}
