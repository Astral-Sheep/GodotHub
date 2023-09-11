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
using System.Collections.Generic;
using Com.Astral.GodotHub.Settings;
using System.Threading;

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

#if DEBUG
		public static string installPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
#else
		public static string installPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
#endif

		private static List<Task<Result>> installs = new List<Task<Result>>();
		private static CancellationTokenSource cts = new CancellationTokenSource();

		public static async Task<Result> InstallAsset(ReleaseAsset pAsset, bool pIsMono)
		{
			if (pAsset == null)
			{
				Debugger.PrintError($"No version passed in method {nameof(InstallAsset)}");
				return Result.Failed;
			}

			Debugger.PrintMessage("Download started");
			LoadingBar.Instance.Ratio = 0.1f;

			#region DOWNLOAD

			HttpClient lClient = new HttpClient();
			HttpResponseMessage lResponse = await lClient.GetAsync(new Uri(pAsset.BrowserDownloadUrl));
			LoadingBar.Instance.Ratio = 0.75f;

			if (lResponse.IsSuccessStatusCode)
			{
				Debugger.PrintMessage("Http request validated: asset successfully downloaded");
			}
			else
			{
				Debugger.PrintError("Failed to access url");
				return Result.Failed;
			}

			#endregion //DOWNLOAD

			#region WRITE_FILE

			string lPath = installPath + "/" + pAsset.Name;
			string lZip = lPath + ".zip";

			try
			{
				FileStream lStream = new FileStream(lZip, System.IO.FileMode.Create);
				LoadingBar.Instance.Ratio = 0.8f;

				await lResponse.Content.CopyToAsync(lStream);
				LoadingBar.Instance.Ratio = 0.85f;

				lStream.Close();
			}
			catch (Exception lException)
			{
				Debugger.PrintError($"Can't write file because of {lException.GetType()}: {lException.Message}");
				return Result.Failed;
			}

			#endregion //WRITE_FILE

			#region INSTALL

			try
			{
				ZipFile.ExtractToDirectory(lZip, pIsMono ? installPath : lPath, true);
				LoadingBar.Instance.Ratio = 0.95f;
			}
			catch (Exception lException)
			{
				Debugger.PrintError($"Can't extract files because of {lException.GetType()}: {lException.Message}");
				return Result.Downloaded;
			}

			if (Config.AutoDeleteDownload)
			{
				try
				{
					File.Delete(lZip);
				}
				catch (Exception lException)
				{
					Debugger.PrintError($"Can't delete zip file because of {lException.GetType()}: {lException.Message}");
					return Result.Installed;
				}
			}

			#endregion //INSTALL

			LoadingBar.Instance.Ratio = 1f;
			LoadingBar.Instance.Complete();

			Debugger.PrintValidation($"{pAsset.Name[0..pAsset.Name.Find(".zip")]} installed successfully");
			return Result.Installed;
		}

		public static void CancelInstall(int pIndex)
		{
			if (pIndex >= installs.Count)
				return;


		}

		public static bool IsInstalled(string pAsset)
		{
			return Directory.Exists(installPath + "/" + pAsset);
		}
	}
}
