using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Debug;
using Com.Astral.GodotHub.Settings;
using Godot;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Environment = System.Environment;
using HttpClient = System.Net.Http.HttpClient;
using Label = Godot.Label;
using OS = Com.Astral.GodotHub.Data.OS;

namespace Com.Astral.GodotHub.Tabs.Installs
{
	public partial class Installer : Control
	{
		protected const float POINT_PIXEL_SIZE = 6f;

		public enum Result
		{
			Installed,
			Downloaded,
			Failed,
			Cancelled,
		}

		public event Action<Installer, Result> Completed;

		public string AssetName { get; protected set; }

		[Export] protected float closeDuration = 0.25f;
		[Export] protected Label versionLabel;
		[Export] protected Label statusLabel;
		[Export] protected ProgressBar loadingBar;
		[Export] protected Button cancelButton;

		protected Source source;
		protected CancellationTokenSource installationSource = new CancellationTokenSource();
		protected CancellationTokenSource animationSource;
		protected bool autoThrowCancel = false;
		protected bool downloading = false;
		protected Vector2 statusPosition;

		public override void _Ready()
		{
			statusPosition = statusLabel.Position;
		}

		public void Init(Source pSource)
		{
			if (pSource.asset == null)
			{
				Debugger.PrintError("Invalid asset passed: [i]null[/i] asset");
				Completed?.Invoke(this, Result.Cancelled);
				return;
			}

			source = pSource;
			AssetName = pSource.asset.Name;
			versionLabel.Text = $"Godot {pSource.version.major}.{pSource.version.minor}";

			if (pSource.version.patch != 0)
			{
				versionLabel.Text += $".{pSource.version.patch}";
			}

			if (pSource.mono)
			{
				versionLabel.Text += " Mono";
			}

			versionLabel.Text += $" {pSource.os}";

			if (pSource.architecture != null)
			{
				versionLabel.Text += $" {pSource.architecture}";
			}

			statusLabel.Text = "In Queue";
			cancelButton.Pressed += OnCancelPressed;
		}

		public async void Install()
		{
			if (source == null)
				return;

			Result lResult = await InstallInternal(installationSource.Token);

			if (lResult == Result.Cancelled)
			{
				statusLabel.Text = "Cancelled";
			}
			else
			{
				cancelButton.Pressed -= OnCancelPressed;
			}

			Completed?.Invoke(this, lResult);

			if (Config.AutoCloseDownload)
			{
				Close();
			}
		}

		protected async Task<Result> InstallInternal(CancellationToken pToken)
		{
			downloading = true;
			animationSource = new CancellationTokenSource();

			#region DOWNLOAD

			loadingBar.Ratio = 0f;
			AnimateStatus("Downloading", animationSource.Token);
			HttpResponseMessage lResponse = null;

			try
			{
				autoThrowCancel = true;
				HttpClient lClient = new HttpClient();
				lResponse = await lClient.GetAsync(new Uri(source.asset.BrowserDownloadUrl), installationSource.Token);
				autoThrowCancel = false;
			}
			catch (OperationCanceledException)
			{
				CancelInstallation(false);
				return Result.Cancelled;
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
				CancelInstallation(true);
				return Result.Failed;
			}

			CancelAnimation();
			loadingBar.Ratio = 1f;
			Thread.Sleep(100);

			if (installationSource.Token.IsCancellationRequested)
			{
				CancelInstallation(true);
				return Result.Cancelled;
			}

			#endregion //DOWNLOAD

			#region INSTALL

			loadingBar.Ratio = 0f;
			animationSource = new CancellationTokenSource();
			AnimateStatus("Installing", animationSource.Token);
			string lZip = Config.DownloadDir + "/" + source.asset.Name;

			#region WRITE_FILE

			try
			{
				FileStream lStream = new FileStream(lZip, System.IO.FileMode.Create);
				loadingBar.Ratio = 0.2f;

				autoThrowCancel = true;
				await lResponse.Content.CopyToAsync(lStream, installationSource.Token);
				autoThrowCancel = false;

				lStream.Close();
				loadingBar.Ratio = Config.AutoDeleteDownload ? 0.6f : 0.75f;

				if (installationSource.Token.IsCancellationRequested)
				{
					CancelInstallation(true, lZip);
					return Result.Cancelled;
				}
			}
			catch (OperationCanceledException)
			{
				CancelInstallation(true, lZip);
				return Result.Cancelled;
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
				CancelInstallation(false, lException is UnauthorizedAccessException ? null : lZip);
				return Result.Failed;
			}

			#endregion //WRITE_FILE

			#region UNZIP

			string lDir = Config.InstallDir;
			string lFile = source.asset.Name[0..^4];
			Debugger.PrintMessage(lFile);

			if (!source.mono && source.os != OS.MacOS)
			{
				if (source.os == OS.Windows)
				{
					lFile = lFile[0..^4];
				}

				lDir += "/" + lFile;
			}

			try
			{
				ZipFile.ExtractToDirectory(lZip, lDir);
				loadingBar.Ratio = Config.AutoDeleteDownload ? 0.8f : 1f;

				if (installationSource.Token.IsCancellationRequested)
				{
					CancelInstallation(true, lZip, lDir);
					return Result.Cancelled;
				}
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
				CancelInstallation(false, lZip, lException is UnauthorizedAccessException ? null : lDir);
				return Result.Downloaded;
			}

			if (Config.AutoDeleteDownload)
			{
				try
				{
					File.Delete(lZip);
					loadingBar.Ratio = 1f;
				}
				catch (Exception lException)
				{
					Debugger.PrintException(lException);
					CancelAnimation();
					return Result.Installed;
				}
			}

			#endregion //UNZIP

			if (Config.AutoCreateShortcut)
			{
				string lShortcutPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/Godot {source.version.major}.{source.version.minor}";

				if (source.version.patch != 0)
				{
					lShortcutPath += $".{source.version.patch}";
				}

				lShortcutPath += ".url";

				StreamWriter lWriter = new StreamWriter(lShortcutPath, true);
				string lAppPath = lDir;
					
				if (source.os == OS.MacOS)
				{
					lAppPath += "/Godot";

					if (source.mono)
					{
						lAppPath += "_mono";
					}

					lAppPath += ".app/Contents/MacOS/Godot";
				}
				else
				{
					if (source.mono)
					{
						lAppPath += $"/{lFile}/{lFile}";

						if (source.os == OS.Linux)
						{
							lAppPath = lAppPath[0..^7] + "." + lAppPath[^6..^0];
						}
					}

					if (source.os == OS.Windows)
					{
						lAppPath += ".exe";
					}
				}
					
				lWriter.WriteLine("[InternetShortcut]");
				lWriter.WriteLine($"URL=file:///{lAppPath}");
				lWriter.WriteLine("IconIndex=0");
				lWriter.WriteLine($"IconFile={lAppPath}");
				lWriter.Close();
			}

			#endregion //INSTALL

			CancelAnimation();
			statusLabel.Text = "Completed";
			downloading = false;
			return Result.Installed;
		}

		protected void CancelInstallation(bool pIsError, string pFile = null, string pDirectory = null)
		{
			CancelAnimation();

			if (!pIsError)
			{
				statusLabel.Text = "Cancelled";
			}

			if (!string.IsNullOrEmpty(pFile))
			{
				try
				{
					File.Delete(pFile);
				}
				catch (Exception lException)
				{
					Debugger.PrintException(lException);
				}
			}

			if (!string.IsNullOrEmpty(pDirectory))
			{
				try
				{
					Directory.Delete(pDirectory);
				}
				catch (Exception lException)
				{
					Debugger.PrintException(lException);
				}
			}
		}

		protected void CancelAnimation()
		{
			animationSource.Cancel();
			statusLabel.Position = statusPosition;
		}

		protected async void AnimateStatus(string pPrefix, CancellationToken pToken)
		{
			List<string> lIcons = new List<string>() {
				"", ".", "..", "...",
			};
			int lIndex = 0;
			float lInitPosition = statusLabel.Position.X;

			await Task.Run(
				() => {
					while (!pToken.IsCancellationRequested)
					{
						statusLabel.Text = $"{pPrefix}{lIcons[lIndex]}";
						statusLabel.Position = new Vector2(
							lInitPosition + (lIndex + 1) * POINT_PIXEL_SIZE,
							statusLabel.Position.Y
						);
						lIndex = (lIndex + 1) % lIcons.Count;
						Thread.Sleep(200);
					}
				},
				pToken
			);
		}

		protected void OnCancelPressed()
		{
			cancelButton.Pressed -= OnCancelPressed;

			if (downloading)
			{
				installationSource.Cancel(autoThrowCancel);
			}
			else
			{
				statusLabel.Text = "Cancelled";
				Completed?.Invoke(this, Result.Cancelled);
			}
		}

		protected void Close()
		{
			CreateTween()
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.Out)
				.TweenProperty(this, "custom_minimum_size:y", 0f, closeDuration)
				.SetDelay(0.75f)
				.Finished += OnClosed;
		}

		protected void OnClosed()
		{
			QueueFree();
		}
	}
}
