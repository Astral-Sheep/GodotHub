﻿using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Debug;
using Com.Astral.GodotHub.Utils;
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
			/// <summary>
			/// The installation process is fully completed
			/// </summary>
			Installed,
			/// <summary>
			/// The installation process stopped after downloading the assets
			/// </summary>
			Downloaded,
			/// <summary>
			/// The installation process wasn't able to download the assets
			/// </summary>
			Failed,
			/// <summary>
			/// The installation process was voluntarily stopped
			/// </summary>
			Cancelled,
		}

		/// <summary>
		/// Download the asset in the given <see cref="Source"/>
		/// </summary>
		/// <param name="pSource"><see cref="Source"/> containing all the data</param>
		/// <param name="pToken"><see cref="CancellationToken"/> used to cancel the operation</param>
		/// <param name="pProgress"><see cref="IProgress{T}"/> used to report progress to the user</param>
		public static async Task<Result> Download(Source pSource, CancellationToken pToken, IProgress<float> pProgress)
		{
			HttpResponseMessage lResponse = null;

			try
			{
				pProgress.Report(0f);

				using (HttpClient lClient = new HttpClient())
				{
					lResponse = await lClient.GetAsync(new Uri(pSource.asset.BrowserDownloadUrl), pToken);
				}
				
				pProgress.Report(0.8f);
			}
			catch (OperationCanceledException)
			{
				return Result.Cancelled;
			}
			catch (Exception lException)
			{
				//To do: create error popup
				Debugger.PrintException(lException);
				return Result.Failed;
			}

			string lZip = AppConfig.DownloadDir + "/" + pSource.asset.Name;

			try
			{
				using (FileStream lStream = new FileStream(lZip, FileMode.Create))
				{
					await lResponse.Content.CopyToAsync(lStream, pToken);
					lStream.Close();
				}

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

				//To do: create error popup
				Debugger.PrintException(lException);
				return Result.Failed;
			}

			pProgress.Report(1f);
			return Result.Downloaded;
		}

		/// <summary>
		/// Unzip a .zip folder of a windows version
		/// </summary>
		/// <param name="pSource"><see cref="Source"/> containing data of the folder to unzip</param>
		/// <param name="pToken"><see cref="CancellationToken"/> used to cancel the operation</param>
		/// <param name="pProgress"><see cref="IProgress{T}"/> used to report progress to the user</param>
		public static async Task<Result> UnzipWindows(Source pSource, CancellationToken pToken, IProgress<float> pProgress)
		{
			string lZip = AppConfig.DownloadDir + "/" + pSource.asset.Name;
			string lDir = AppConfig.InstallDir;

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

			if (AppConfig.AutoCreateShortcut)
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
					//To do: create error popup
					Debugger.PrintException(lException);
					return Result.Installed;
				}
			}
#endif

			return Result.Installed;
		}
		/// <summary>
		/// Unzip a .zip folder of a linux version
		/// </summary>
		/// <param name="pSource"><see cref="Source"/> containing data of the folder to unzip</param>
		/// <param name="pToken"><see cref="CancellationToken"/> used to cancel the operation</param>
		/// <param name="pProgress"><see cref="IProgress{T}"/> used to report progress to the user</param>
		public static async Task<Result> UnzipLinux(Source pSource, CancellationToken pToken, IProgress<float> pProgress)
		{
			string lZip = AppConfig.DownloadDir + "/" + pSource.asset.Name;
			string lDir = AppConfig.InstallDir;

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

				if (!PathT.RenameFolder(lOriginalFolder, lFormattedFolder))
				{
					//To do: create error popup
					Debugger.PrintError($"Can't rename folder {lOriginalFolder}");
					return Result.Failed;
				}
			}

#if GODOT_LINUXBSD
			InstallsData.AddVersion(pSource.mono ? lFormattedFolder : lDir, false);

			if (AppConfig.AutoCreateShortcut)
			{
				try
				{
					await Task.Run(
						() => {
							CreateShortcut(PathT.GetExeFromFolder(lFormattedFolder, pSource.os), pSource.version);
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
					//To do: create error popup
					Debugger.PrintException(lException);
					return Result.Installed;
				}
			}
#endif

			return Result.Installed;
		}

		/// <summary>
		/// Unzip a .zip folder of a macos version
		/// </summary>
		/// <param name="pSource"><see cref="Source"/> containing data of the folder to unzip</param>
		/// <param name="pToken"><see cref="CancellationToken"/> used to cancel the operation</param>
		/// <param name="pProgress"><see cref="IProgress{T}"/> used to report progress to the user</param>
		public static async Task<Result> UnzipMacOS(Source pSource, CancellationToken pToken, IProgress<float> pProgress)
		{
			string lZip = AppConfig.DownloadDir + "/" + pSource.asset.Name;
			string lDir = AppConfig.InstallDir;
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

			if (AppConfig.AutoCreateShortcut)
			{
				try
				{
					await Task.Run(
						() => {
							CreateShortcut(PathT.GetExeFromFolder(lFormattedFolder, pSource.os), pSource.version);
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
					//To do: create error popup
					Debugger.PrintException(lException);
					return Result.Installed;
				}
			}
#endif

			return Result.Installed;
		}

		private static async Task<Result> Unzip(string pZip, string pDir, CancellationToken pToken, IProgress<float> pProgress)
		{
			try
			{
				await Task.Run(
					() => ZipFile.ExtractToDirectory(pZip, pDir),
					pToken
				);
				pProgress.Report(AppConfig.AutoDeleteZip ? 0.7f : 1f);
			}
			catch (OperationCanceledException)
			{
				CancelUnzip(pZip, pDir);
				return Result.Cancelled;
			}
			catch (Exception lException)
			{
				//To do: create error popup
				Debugger.PrintException(lException);
				CancelUnzip(pZip, lException is UnauthorizedAccessException ? null : pDir);
				return Result.Failed;
			}

			if (AppConfig.AutoDeleteZip)
			{
				try
				{
					File.Delete(pZip);
					pProgress.Report(1f);
				}
				catch (OperationCanceledException)
				{
					CancelUnzip(pZip, pDir);
					return Result.Cancelled;
				}
				catch (Exception lException)
				{
					//To do: create error popup
					Debugger.PrintException(lException);
					return Result.Installed;
				}
			}

			return Result.Installed;
		}

		private static void CreateShortcut(string pExecutable, Version pVersion)
		{
			using (StreamWriter lWriter = new StreamWriter($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/Godot {pVersion}.url", false))
			{
				lWriter.Write(
					$"[InternetShortcut]{PathT.EOL}" +
					$"URL=file:///{pExecutable}{PathT.EOL}" +
					$"IconIndex=0{PathT.EOL}" +
					$"IconFile={pExecutable}{PathT.EOL}"
				);
				lWriter.Close();
			}
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
					//To do: create error popup
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
					//To do: create error popup
					Debugger.PrintException(lException);
				}
			}
		}
	}
}
