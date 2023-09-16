using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Debug;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Environment = System.Environment;
using HttpClient = System.Net.Http.HttpClient;
using Version = Com.Astral.GodotHub.Data.Version;

namespace Com.Astral.GodotHub.Tabs.Installs
{
	public static class InstallT
	{
		public enum Result
		{
			Installed,
			Downloaded,
			Failed,
			Cancelled,
		}

		public static async Task<Result> Download(Source pSource, CancellationToken pToken, IProgress<float> pProgress)
		{
			HttpResponseMessage lResponse = null;

			try
			{
				pProgress.Report(0f);
				HttpClient lClient = new HttpClient();
				lResponse = await lClient.GetAsync(new Uri(pSource.asset.BrowserDownloadUrl), pToken);
				pProgress.Report(0.8f);
			}
			catch (OperationCanceledException)
			{
				return Result.Cancelled;
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
				return Result.Failed;
			}

			string lZip = Config.DownloadDir + "/" + pSource.asset.Name;

			try
			{
				FileStream lStream = new FileStream(lZip, FileMode.Create);
				await lResponse.Content.CopyToAsync(lStream, pToken);
				lStream.Close();
				pProgress.Report(0.9f);
			}
			catch (OperationCanceledException)
			{
				CancelUnzip(lZip, null);
				return Result.Cancelled;
			}
			catch (Exception lException)
			{
				if (lException is not UnauthorizedAccessException)
				{
					CancelUnzip(lZip, null);
				}

				Debugger.PrintException(lException);
				return Result.Failed;
			}

			pProgress.Report(1f);
			return Result.Downloaded;
		}

		public static async Task<Result> UnzipWindows(Source pSource, CancellationToken pToken, IProgress<float> pProgress)
		{
			string lZip = Config.DownloadDir + "/" + pSource.asset.Name;
			string lDir = Config.InstallDir;

			if (!pSource.mono)
			{
				lDir += $"/{pSource.asset.Name[..^8]}";
			}

			Result lResult = await Unzip(lZip, lDir, pToken, pProgress);

			if (lResult != Result.Installed)
			{
				return lResult;
			}

#if GODOT_WINDOWS
			string lFolder = pSource.mono ? lZip[..^4] : lDir;
			InstallsData.AddVersion(lFolder, false);

			if (Config.AutoCreateShortcut)
			{
				try
				{
					await Task.Run(
						() => {
							CreateShortcut(PathT.GetExeFromFolder(lFolder, pSource.os), pSource.version);
						},
						pToken
					);
				}
				catch (OperationCanceledException)
				{
					CancelUnzip(lZip, lDir);
					return Result.Cancelled;
				}
				catch (Exception lException)
				{
					Debugger.PrintException(lException);
					return Result.Installed;
				}
			}
#endif

			return Result.Installed;
		}

		public static async Task<Result> UnzipLinux(Source pSource, CancellationToken pToken, IProgress<float> pProgress)
		{
			string lZip = Config.DownloadDir + "/" + pSource.asset.Name;
			string lDir = Config.InstallDir;

			if (!pSource.mono)
			{
				lDir += $"/{pSource.asset.Name[..^4]}";
			}

			Result lResult = await Unzip(lZip, lDir, pToken, pProgress);

			if (lResult != Result.Installed)
			{
				return lResult;
			}

			string lFormattedFolder = lZip[..^4];

			if (pSource.mono)
			{
				string lOriginalFolder = lFormattedFolder;
				lFormattedFolder = PathT.FormatLinuxFolder(lFormattedFolder);

				if (!await Task.Run(() => PathT.RenameFolder(lOriginalFolder, lFormattedFolder)))
				{
					Debugger.PrintError($"Can't rename folder {lOriginalFolder}");
					return Result.Failed;
				}
			}

#if GODOT_LINUXBSD
			InstallsData.AddVersion(pSource.mono ? lFormattedFolder : lDir, false);

			if (Config.AutoCreateShortcut)
			{
				await Task.Run(() => {
					CreateShortcut(PathT.GetExeFromFolder(lFormattedFolder, pSource.os), pSource.version);
				});
			}
#endif

			return Result.Installed;
		}

		public static async Task<Result> UnzipMacOS(Source pSource, CancellationToken pToken, IProgress<float> pProgress)
		{
			string lZip = Config.DownloadDir + "/" + pSource.asset.Name;
			string lDir = Config.InstallDir;
			Result lResult = await Unzip(lZip, lDir, pToken, pProgress);

			if (lResult != Result.Installed)
			{
				return lResult;
			}

			string lOriginalFolder = PathT.GetMacOSFolderFromZip(lZip, pSource.mono);
			string lFormattedFolder = PathT.FormatMacOSFolder(lOriginalFolder, pSource.version, pSource.mono);
			PathT.RenameFolder(lOriginalFolder, lFormattedFolder);

#if GODOT_MACOS
			InstallsData.AddVersion(lFormattedFolder, false);

			if (Config.AutoCreateShortcut)
			{
				await Task.Run(() => {
					CreateShortcut(PathT.GetExeFromFolder(lFormattedFolder, pSource.os), pSource.version);
				});
			}
#endif

			return Result.Installed;
		}

		private static async Task<Result> Unzip(string pZip, string pDir, CancellationToken pToken, IProgress<float> pProgress)
		{
			try
			{
				await Task.Run(() => ZipFile.ExtractToDirectory(pZip, pDir), pToken);
				pProgress.Report(Config.AutoDeleteZip ? 0.7f : 1f);
			}
			catch (OperationCanceledException)
			{
				CancelUnzip(pZip, pDir);
				return Result.Cancelled;
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
				CancelUnzip(pZip, lException is UnauthorizedAccessException ? null : pDir);
				return Result.Failed;
			}

			if (Config.AutoDeleteZip)
			{
				try
				{
					await Task.Run(() => File.Delete(pZip), pToken);
					pProgress.Report(1f);
				}
				catch (OperationCanceledException)
				{
					CancelUnzip(pZip, pDir);
					return Result.Cancelled;
				}
				catch (Exception lException)
				{
					Debugger.PrintException(lException);
					return Result.Installed;
				}
			}

			return Result.Installed;
		}

		private static void CreateShortcut(string pExecutable, Version pVersion)
		{
			StreamWriter lWriter = new StreamWriter(
				$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/Godot {pVersion}.url",
				true
			);
			lWriter.WriteLine("[InternetShortcut]");
			lWriter.WriteLine($"URL=file:///{pExecutable}");
			lWriter.WriteLine("IconIndex=0");
			lWriter.WriteLine($"IconFile={pExecutable}");
			lWriter.Close();
		}

		private static void CancelUnzip(string pZip, string pDir)
		{
			if (pZip != null && File.Exists(pZip))
			{
				try
				{
					File.Delete(pZip);
				}
				catch (Exception lException)
				{
					Debugger.PrintException(lException);
				}
			}

			if (pDir != null && Directory.Exists(pDir))
			{
				try
				{
					File.Delete(pDir);
				}
				catch (Exception lException)
				{
					Debugger.PrintException(lException);
				}
			}
		}
	}
}
