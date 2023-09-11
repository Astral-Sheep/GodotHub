using Godot;
using Octokit;

namespace Com.Astral.GodotHub
{
	public class Asset
	{
		public ReleaseAsset asset;
		public (int major, int minor, int patch) version;
		public bool mono;
		public OS os;
		public Architecture? architecture;

		public Asset(ReleaseAsset pAsset, (int major, int minor, int patch) pVersion, bool pMono, OS pOS, Architecture? pArchitecture = null)
		{
			asset = pAsset;
			version = pVersion;
			mono = pMono;
			os = pOS;
			architecture = pArchitecture;
		}

		public static (int, int, int) GetVersion(Release pRelease)
		{
			string lVersion = pRelease.Name[0..pRelease.Name.Find("-")];
			return (lVersion[0], lVersion[2], lVersion.Length > 3 ? lVersion[4] : 0);
		}

		public static bool IsMono(ReleaseAsset pAsset)
		{
			return pAsset.Name.Contains("mono");
		}

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
