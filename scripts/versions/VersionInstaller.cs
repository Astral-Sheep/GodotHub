using Com.Astral.GodotHub.Debug;
using Octokit;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using HttpClient = System.Net.Http.HttpClient;

namespace Com.Astral.GodotHub.Releases
{
	public static class VersionInstaller
	{
		public enum InstallationResult
		{
			Installed,
			Downloaded,
			Failed,
			Cancelled,
		}

#if DEBUG
		public static string installPath = Environment.GetEnvironmentVariable("USERPROFILE") + @"\Downloads";
#else
		public static string installPath = @"C:\Program Files";
#endif

		public static async Task<InstallationResult> InstallVersion(string pVersion, string pOS, bool pMono)
		{
			Release lRelease = GodotRepo.GetRelease(pVersion);

			if (lRelease == null)
			{
				Debugger.PrintError($"Godot {pVersion} doesn't exists. Cancelling installation");
				return InstallationResult.Failed;
			}

			string lFileName = GetAssetName(pVersion, pOS, pMono);

			if (Directory.Exists(installPath + @"\" + lFileName))
			{
				return InstallationResult.Cancelled;
			}

			lFileName += ".zip";

			for (int i = 0; i < lRelease.Assets.Count; i++)
			{
				if (lRelease.Assets[i].Name == lFileName)
				{
					return await InstallAsset(lRelease.Assets[i], pMono);
				}
			}

			Debugger.PrintError($"{lFileName} doesn't exists");
			return InstallationResult.Failed;
		}

		public static async Task<InstallationResult> InstallAsset(ReleaseAsset pAsset, bool pIsMono)
		{
			if (pAsset == null)
			{
				Debugger.PrintError($"No version passed in method {nameof(InstallAsset)}");
				return InstallationResult.Failed;
			}

			HttpClient lClient = new HttpClient();
			HttpResponseMessage lResponse = await lClient.GetAsync(new Uri(pAsset.BrowserDownloadUrl));

			if (lResponse.IsSuccessStatusCode)
			{
				Debugger.PrintMessage("Http request validated");
			}
			else
			{
				Debugger.PrintError("Failed to access url");
				return InstallationResult.Failed;
			}

			string lPath = installPath + @"\" + pAsset.Name;
			string lZip = lPath + ".zip";

			try
			{
				FileStream lStream = new FileStream(lZip, System.IO.FileMode.Create);
				await lResponse.Content.CopyToAsync(lStream);
				lStream.Close();
			}
			catch (Exception lException)
			{
				Debugger.PrintError($"{lException.GetType()}: {lException.Message}");
				return InstallationResult.Failed;
			}

			try
			{
				ZipFile.ExtractToDirectory(lZip, pIsMono ? installPath : lPath, true);
			}
			catch (Exception lException)
			{
				Debugger.PrintError($"{lException.GetType()}: {lException.Message}");
				return InstallationResult.Downloaded;
			}

			try
			{
				File.Delete(lZip);
			}
			catch (Exception lException)
			{
				Debugger.PrintError($"{lException.GetType()}: {lException.Message}");
				return InstallationResult.Installed;
			}

			return InstallationResult.Installed;
		}

		public static string GetAssetName(string pVersion, string pOS, bool pMono)
		{
			return $"Godot_v{pVersion}_{(pMono ? "mono_" : "")}{pOS}";
		}
	}
}
