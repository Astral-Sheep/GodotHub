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
using HttpClient = System.Net.Http.HttpClient;
using Label = Godot.Label;

namespace Com.Astral.GodotHub.Releases
{
	public partial class Installer : Control
	{
		public enum Result
		{
			Installed,
			Downloaded,
			Failed,
			Cancelled,
		}

		public event Action<Result> Completed;

		[Export] protected float closeDuration = 0.25f;
		[Export] protected Label versionLabel;
		[Export] protected Label statusLabel;
		[Export] protected ProgressBar loadingBar;
		[Export] protected Button cancelButton;

		protected CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		protected bool completed = false;

		public async void Install(Asset pAsset)
		{
			if (pAsset.asset == null)
			{
				Debugger.PrintError("Invalid asset passed: [code]null[/code] asset");
				Completed?.Invoke(Result.Cancelled);
				return;
			}

			versionLabel.Text = $"Godot {pAsset.version.major}.{pAsset.version.minor}";

			if (pAsset.version.patch == 0)
			{
				versionLabel.Text += $".{pAsset.version.patch}";
			}

			if (pAsset.mono)
			{
				versionLabel.Text += " Mono";
			}

			versionLabel.Text += $" {pAsset.os}";

			if (pAsset.architecture != null)
			{
				versionLabel.Text += $" {pAsset.architecture}";
			}

			await InstallInternal(pAsset.asset, pAsset.mono, cancellationTokenSource.Token);
		}

		protected async Task InstallInternal(ReleaseAsset pAsset, bool pMono, CancellationToken pToken)
		{
			#region DOWNLOAD

			loadingBar.Ratio = 0f;
			CancellationTokenSource lSource = new CancellationTokenSource();
			AnimateStatus("Downloading", lSource.Token);
			HttpClient lClient = new HttpClient();
			HttpResponseMessage lResponse = await lClient.GetAsync(new Uri(pAsset.BrowserDownloadUrl));
			lSource.Cancel();

			if (CheckToken(pToken))
				return;

			loadingBar.Ratio = 1f;
			Thread.Sleep(50);

			#endregion //DOWNLOAD

			#region INSTALL

			loadingBar.Ratio = 0f;
			lSource = new CancellationTokenSource();
			AnimateStatus("Installing", lSource.Token);
			string lZip = Config.DownloadDir + "/" + pAsset.Name;

			#region WRITE_FILE

			try
			{
				FileStream lStream = new FileStream(lZip, System.IO.FileMode.Create);
				loadingBar.Ratio = 0.2f;
				await lResponse.Content.CopyToAsync(lStream);
				lStream.Close();
				loadingBar.Ratio = Config.AutoDeleteDownload ? 0.6f : 0.75f;

				if (CheckToken(pToken))
				{
					File.Delete(lZip);
					return;
				}
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
				Completed?.Invoke(Result.Failed);
				return;
			}

			#endregion //WRITE_FILE

			#region UNZIP

			try
			{
				string lDir = pMono ? Config.InstallDir : Config.InstallDir + "/" + pAsset.Name[0..^4];
				ZipFile.ExtractToDirectory(lZip, lDir);
				loadingBar.Ratio = Config.AutoDeleteDownload ? 0.8f : 1f;

				if (CheckToken(pToken))
				{
					Directory.Delete(lDir);
					File.Delete(lZip);
					return;
				}
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
				Completed?.Invoke(Result.Downloaded);
				return;
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
					Completed?.Invoke(Result.Installed);
					return;
				}
			}

			#endregion //UNZIP

			#endregion //INSTALL

			lSource.Cancel();
			Completed?.Invoke(Result.Installed);
			Close();
		}

		protected async void AnimateStatus(string pPrefix, CancellationToken pToken)
		{
			List<string> lIcons = new List<string>() {
				".", "..", "...",
			};
			int lIndex = 0;

			await Task.Run(
				() => {
					while (!pToken.IsCancellationRequested)
					{
						statusLabel.Text = $"{pPrefix}{lIcons[lIndex]}";
						lIndex = (lIndex + 1) % lIcons.Count;
						Thread.Sleep(250);
					}
				},
				pToken
			);
		}

		protected bool CheckToken(CancellationToken pToken)
		{
			if (!pToken.IsCancellationRequested)
				return false;

			Completed?.Invoke(Result.Cancelled);
			//Close();
			return true;
		}

		protected void OnCancelButtonPressed()
		{
			cancellationTokenSource.Cancel();
		}

		protected void Close()
		{
			Debugger.PrintMessage("Closing");
			CreateTween()
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.Out)
				.TweenProperty(this, "size:y", 0f, closeDuration)
				.SetDelay(0.75f)
				.Finished += OnClosed;
		}

		protected void OnClosed()
		{
			Debugger.PrintMessage("Closed");
			QueueFree();
		}
	}
}
