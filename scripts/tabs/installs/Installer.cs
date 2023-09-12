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

using HttpClient = System.Net.Http.HttpClient;
using Label = Godot.Label;

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

		protected CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

		public async void Install(Source pSource)
		{
			if (pSource.asset == null)
			{
				Debugger.PrintError("Invalid asset passed: [i]null[/i] asset");
				Completed?.Invoke(this, Result.Cancelled);
				return;
			}

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

			Result lResult = await InstallInternal(
				pSource.asset,
				pSource.mono,
				cancellationTokenSource.Token
			);
			Completed?.Invoke(this, lResult);
			//Close();
		}

		protected async Task<Result> InstallInternal(ReleaseAsset pAsset, bool pMono, CancellationToken pToken)
		{
			Vector2 lStatusPosition = statusLabel.Position;

			#region DOWNLOAD

			loadingBar.Ratio = 0f;
			CancellationTokenSource lTokenSource = new CancellationTokenSource();
			AnimateStatus("Downloading", lTokenSource.Token);
			HttpClient lClient = new HttpClient();
			HttpResponseMessage lResponse = await lClient.GetAsync(new Uri(pAsset.BrowserDownloadUrl));
			lTokenSource.Cancel();
			statusLabel.Position = lStatusPosition;

			if (pToken.IsCancellationRequested)
			{
				return Result.Cancelled;
			}

			loadingBar.Ratio = 1f;
			Thread.Sleep(50);

			#endregion //DOWNLOAD

			#region INSTALL

			loadingBar.Ratio = 0f;
			lTokenSource = new CancellationTokenSource();
			AnimateStatus("Installing", lTokenSource.Token);
			string lZip = Config.DownloadDir + "/" + pAsset.Name;

			#region WRITE_FILE

			try
			{
				FileStream lStream = new FileStream(lZip, System.IO.FileMode.Create);
				loadingBar.Ratio = 0.2f;
				await lResponse.Content.CopyToAsync(lStream);
				lStream.Close();
				loadingBar.Ratio = Config.AutoDeleteDownload ? 0.6f : 0.75f;

				if (pToken.IsCancellationRequested)
				{
					File.Delete(lZip);
					return Result.Cancelled;
				}
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
				return Result.Failed;
			}

			#endregion //WRITE_FILE

			#region UNZIP

			try
			{
				string lDir = pMono ? Config.InstallDir : Config.InstallDir + "/" + pAsset.Name[0..^4];
				ZipFile.ExtractToDirectory(lZip, lDir);
				loadingBar.Ratio = Config.AutoDeleteDownload ? 0.8f : 1f;

				if (pToken.IsCancellationRequested)
				{
					Directory.Delete(lDir);
					File.Delete(lZip);
					return Result.Cancelled;
				}
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
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
					return Result.Installed;
				}
			}

			#endregion //UNZIP

			#endregion //INSTALL

			lTokenSource.Cancel();
			statusLabel.Position = lStatusPosition;
			statusLabel.Text = "Completed";
			return Result.Installed;
		}

		protected async void AnimateStatus(string pPrefix, CancellationToken pToken)
		{
			List<string> lIcons = new List<string>() {
				".", "..", "...",
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
						Thread.Sleep(250);
					}
				},
				pToken
			);
		}

		protected void OnCancelButtonPressed()
		{
			cancellationTokenSource.Cancel();
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
