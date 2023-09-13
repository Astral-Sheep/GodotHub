using Godot;
using Octokit;

namespace Com.Astral.GodotHub.Data
{
	public class Source
	{
		public ReleaseAsset asset;
		public (int major, int minor, int patch) version;
		public bool mono;
		public OS os;
		public Architecture? architecture;

		public Source() { }

		public Source(ReleaseAsset pAsset, (int major, int minor, int patch) pVersion, bool pMono, OS pOS, Architecture? pArchitecture = null)
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
			return (
				int.Parse(lVersion[0].ToString()),
				int.Parse(lVersion[2].ToString()),
				lVersion.Length > 3 ? int.Parse(lVersion[4].ToString()) : 0
			);
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
