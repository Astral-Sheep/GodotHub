using Com.Astral.GodotHub.Data;
using Godot;

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

		protected Tween tween;

		public override void _Ready()
		{
			panel.Position = new Vector2(-panel.Size.X, panel.Position.Y);
			background.MouseFilter = MouseFilterEnum.Ignore;
			background.SelfModulate = new Color(background.SelfModulate, 0f);

			CreateCustomTween(Tween.EaseType.Out);
			ReleaseItem.InstallClicked += Install;
			openButton.Pressed += Open;
		}

		protected override void Dispose(bool pDisposing)
		{
			if (pDisposing)
			{
				ReleaseItem.InstallClicked -= Install;
			}
		}

		public void Install(ReleaseItem pItem, Source pAsset)
		{
			Installer lInstaller = installerScene.Instantiate<Installer>();
			installerContainer.AddChild(lInstaller);
			pItem.Connect(lInstaller);
			lInstaller.Install(pAsset);
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
