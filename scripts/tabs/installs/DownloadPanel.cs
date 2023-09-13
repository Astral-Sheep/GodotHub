using Com.Astral.GodotHub.Data;
using Godot;
using System.Collections;
using System.Collections.Generic;

namespace Com.Astral.GodotHub.Tabs.Installs
{
	public partial class DownloadPanel : Control
	{
		[Export] protected float openDuration = 0.2f;
		[Export] protected Control panel;
		[Export] protected Button background;
		[ExportGroup("Installation")]
		[Export] protected PackedScene installerScene;
		[Export] protected Control installerContainer;
		[ExportGroup("Buttons")]
		[Export] protected Button openButton;
		[Export] protected Button closeButton;

		protected List<Installer> installers = new List<Installer>();
		protected Tween tween;

		public override void _Ready()
		{
			panel.Position = new Vector2(-panel.Size.X, panel.Position.Y);
			background.MouseFilter = MouseFilterEnum.Ignore;
			background.SelfModulate = new Color(background.SelfModulate, 0f);

			CreateCustomTween(Tween.EaseType.Out);
			ReleaseItem.InstallClicked += Install;
			openButton.Pressed += Open;
			Visible = true;
		}

		protected override void Dispose(bool pDisposing)
		{
			if (pDisposing)
			{
				ReleaseItem.InstallClicked -= Install;
			}
		}

		public void Install(ReleaseItem pItem, Source pSource)
		{
			Installer lInstaller = installerScene.Instantiate<Installer>();
			installerContainer.AddChild(lInstaller);
			installers.Add(lInstaller);
			pItem.Connect(lInstaller);
			lInstaller.Init(pSource);
			lInstaller.Completed += OnInstallerCompleted;

			if (installers.Count < 2)
			{
				lInstaller.Install();
			}
		}

		protected void OnInstallerCompleted(Installer pInstaller, Installer.Result _)
		{
			pInstaller.Completed -= OnInstallerCompleted;
			installers.Remove(pInstaller);

			if (installers.Count > 0)
			{
				Installer lInstaller = installers[0];
				lInstaller.Install();
			}
		}

		protected void Open()
		{
			openButton.Pressed -= Open;
			openButton.Pressed += Close;
			closeButton.Pressed += Close;
			background.Pressed += Close;

			background.MouseFilter = MouseFilterEnum.Stop;
			ResetTween(Tween.EaseType.Out);
			tween.TweenProperty(background, "self_modulate:a", 1f, openDuration);
			tween
				.TweenProperty(panel, "position:x", 0, openDuration)
				.Finished += OnTweenFinished;
			tween.Play();
		}

		protected void Close()
		{
			background.Pressed -= Close;
			closeButton.Pressed -= Close;
			openButton.Pressed -= Close;
			openButton.Pressed += Open;

			background.MouseFilter = MouseFilterEnum.Ignore;
			ResetTween(Tween.EaseType.In);
			tween.TweenProperty(background, "self_modulate:a", 0f, openDuration);
			tween
				.TweenProperty(panel, "position:x", -panel.Size.X, openDuration)
				.Finished += OnTweenFinished;
			tween.Play();
		}

		protected void OnTweenFinished()
		{
			tween.Stop();
		}

		protected void ResetTween(Tween.EaseType pEase)
		{
			if (tween != null && tween.IsValid())
			{
				tween.Kill();
			}

			CreateCustomTween(pEase);
		}

		protected void CreateCustomTween(Tween.EaseType pEase)
		{
			tween = CreateTween()
				.SetTrans(Tween.TransitionType.Cubic)
				.SetEase(pEase);
			tween.Stop();
		}
	}
}
