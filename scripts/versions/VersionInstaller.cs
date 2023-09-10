using Com.Astral.GodotHub.Debug;
using Godot;
using Octokit;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using HttpClient = System.Net.Http.HttpClient;
using Environment = System.Environment;

namespace Com.Astral.GodotHub.Releases
{
	public static class VersionInstaller
	{
		public enum Result
		{
			Installed,
			Downloaded,
			Failed,
			Cancelled,
		}

		public enum OS
		{
			Windows = 0,
			Linux = 1,
			MacOS = 2,
		}

		public enum Bit
		{
			x32 = 0,
			x64 = 1,
		}

#if DEBUG
		public static string installPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
#else
		public static string installPath = @"C:\Program Files";
#endif

		public static async Task<Result> InstallAsset(ReleaseAsset pAsset, bool pIsMono)
		{
			if (pAsset == null)
			{
				Debugger.PrintError($"No version passed in method {nameof(InstallAsset)}");
				return Result.Failed;
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
				return Result.Failed;
			}

			string lPath = installPath + "/" + pAsset.Name;
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
				return Result.Failed;
			}

			try
			{
				ZipFile.ExtractToDirectory(lZip, pIsMono ? installPath : lPath, true);
			}
			catch (Exception lException)
			{
				Debugger.PrintError($"{lException.GetType()}: {lException.Message}");
				return Result.Downloaded;
			}

			try
			{
				File.Delete(lZip);
			}
			catch (Exception lException)
			{
				Debugger.PrintError($"{lException.GetType()}: {lException.Message}");
				return Result.Installed;
			}

			Debugger.PrintValidation($"{pAsset.Name[0..pAsset.Name.Find(".zip")]} installed successfully");
			return Result.Installed;
		}

		public static bool IsInstalled(string pAsset)
		{
			return Directory.Exists(installPath + "/" + pAsset);
		}
	}
}
