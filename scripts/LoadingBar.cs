using Com.Astral.GodotHub.Debug;
using Godot;

namespace Com.Astral.GodotHub
{
	public partial class LoadingBar : ProgressBar
	{
		public static LoadingBar Instance { get; private set; }

		//RGBA color as (R << 24) + (G << 16) + (B << 8) + A
		[Export] protected Color completedColor = new Color((55u << 24) + (157u << 16) + (166u << 8) + 255u);
		//RGBA color as (R << 24) + (G << 16) + (B << 8) + A
		[Export] protected Color canceledColor = new Color((212u << 24) + (80u << 16) + (79u << 8) + 255u);
		protected Color baseColor;
		protected StyleBoxFlat fillStyleBox;
		protected Tween tween;

		private LoadingBar()
		{
			if (Instance != null)
			{
				GD.PushWarning($"{nameof(LoadingBar)} instance already exists, destroying the last added");
				Free();
				return;
			}

			Instance = this;
		}

		public override void _Ready()
		{
			Value = MinValue;
			fillStyleBox = (StyleBoxFlat)GetThemeStylebox("fill");
			baseColor = fillStyleBox.BgColor;
			CreateCustomTween();
		}

		protected override void Dispose(bool pDisposing)
		{
			if (pDisposing && Instance == this)
			{
				Instance = null;
			}
		}

		public void Complete()
		{
			Disappear();
		}

		public void Cancel()
		{
			Disappear();
		}

		protected void Disappear()
		{
			tween
				.SetTrans(Tween.TransitionType.Cubic)
				.SetEase(Tween.EaseType.In)
				.TweenProperty(fillStyleBox, "bg_color", new Color(baseColor, 0f), 0.5)
				.SetDelay(0.25);

			tween.Finished += OnAnimationFinished;
			tween.Play();
		}

		protected void OnAnimationFinished()
		{
			tween.Finished -= OnAnimationFinished;
			ResetTween();
			Ratio = 0f;
			fillStyleBox.BgColor = baseColor;
		}

		protected void ResetTween()
		{
			if (tween.IsValid())
			{
				tween.Kill();
			}

			CreateCustomTween();
		}

		protected void CreateCustomTween()
		{
			tween = CreateTween().SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
			tween.Stop();
		}
	}
}
