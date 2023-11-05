using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Debug;
using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Label = Godot.Label;
using OS = Com.Astral.GodotHub.Data.OS;

namespace Com.Astral.GodotHub.Tabs.Installs
{
	public partial class Installer : Control
	{
		protected const float POINT_PIXEL_SIZE = 6f;

		/// <summary>
		/// Event called when the installation has ended.<br/>
		/// <see cref="Installer"/> corresponds to the instance that installed the <see cref="Source"/>.<br/>
		/// <see cref="InstallT.Result"/> corresponds to the completion status.
		/// </summary>
		public event Action<Installer, InstallT.Result> Completed;

		/// <summary>
		/// The name of the asset contained by the <see cref="Source"/>
		/// </summary>
		public string AssetName { get; protected set; }

		[Export] protected float closeDuration = 0.25f;

		[ExportGroup("Data")]
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

		/// <summary>
		/// Initialize this instance by giving it a <see cref="Source"/>
		/// </summary>
		public void Init(Source pSource)
		{
			if (pSource.asset == null)
			{
				Debugger.LogError("Invalid asset passed: [i]null[/i] asset");
				statusLabel.Text = "Cancelled";
				Completed?.Invoke(this, InstallT.Result.Cancelled);
				return;
			}

			source = pSource;
			AssetName = pSource.asset.Name;
			versionLabel.Text = $"Godot {pSource.version}";

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

		/// <summary>
		/// Start the installation process. Need to call <see cref="Init(Source)"/> beforehand to be
		/// able to install anything
		/// </summary>
		public async void Install()
		{
			if (source == null)
			{
				statusLabel.Text = "Failed";
				Completed?.Invoke(this, InstallT.Result.Failed);
				return;
			}

			InstallT.Result lResult = await InstallInternal();

			if (lResult == InstallT.Result.Cancelled)
			{
				statusLabel.Text = "Cancelled";
			}
			else
			{
				cancelButton.Pressed -= OnCancelPressed;
			}

			Completed?.Invoke(this, lResult);

			if (AppConfig.AutoCloseDownload)
			{
				Close(0.75f);
			}
			else
			{
				cancelButton.Pressed += OnClosePressed;
			}
		}

		protected async Task<InstallT.Result> InstallInternal()
		{
			downloading = true;
			animationSource = new CancellationTokenSource();

			loadingBar.Ratio = 0f;
			IProgress<float> lProgress = new Progress<float>((float f) => { loadingBar.Ratio = f; });
			AnimateStatus("Downloading", animationSource.Token);

			autoThrowCancel = true;
			InstallT.Result lResult = await InstallT.Download(source, installationSource.Token, lProgress);
			autoThrowCancel = false;

			if (lResult == InstallT.Result.Cancelled)
			{
				CancelInstallation(false);
				return InstallT.Result.Cancelled;
			}
			else if (lResult == InstallT.Result.Failed)
			{
				CancelInstallation(true);
				return InstallT.Result.Failed;
			}

			CancelAnimation();
			Thread.Sleep(100);

			if (installationSource.Token.IsCancellationRequested)
			{
				CancelInstallation(true);
				return InstallT.Result.Cancelled;
			}

			lProgress.Report(0f);
			animationSource = new CancellationTokenSource();
			AnimateStatus("Installing", animationSource.Token);

			autoThrowCancel = true;
			lResult = source.os switch {
				OS.Windows => await InstallT.UnzipWindows(source, installationSource.Token, lProgress),
				OS.Linux => await InstallT.UnzipLinux(source, installationSource.Token, lProgress),
				OS.MacOS => await InstallT.UnzipMacOS(source, installationSource.Token, lProgress),
				_ => InstallT.Result.Failed,
			};
			autoThrowCancel = false;

			if (lResult == InstallT.Result.Cancelled)
			{
				CancelInstallation(false);
			}
			else if (lResult == InstallT.Result.Failed)
			{
				CancelInstallation(true);
			}

			CancelAnimation();
			statusLabel.Text = "Completed";
			downloading = false;
			return InstallT.Result.Installed;
		}

		protected void CancelInstallation(bool pIsError)
		{
			CancelAnimation();

			if (!pIsError)
			{
				statusLabel.Text = "Cancelled";
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

		protected void Close(float pDelay)
		{
			CreateTween()
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.Out)
				.TweenProperty(this, "custom_minimum_size:y", 0f, closeDuration)
				.SetDelay(pDelay)
				.Finished += QueueFree;
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
				Completed?.Invoke(this, InstallT.Result.Cancelled);
				cancelButton.Pressed += OnClosePressed;
			}
		}

		protected void OnClosePressed()
		{
			cancelButton.Pressed -= OnClosePressed;
			Close(0f);
		}
	}
}
