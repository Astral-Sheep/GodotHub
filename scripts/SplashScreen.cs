using Godot;

namespace Com.Astral.GodotHub
{
	public partial class SplashScreen : ColorRect
	{
		[Export] protected float fadeDuration = 0.2f;

		public override void _Ready()
		{
			Main.Instance.Initialized += FadeOut;
		}

		protected void FadeOut()
		{
			Main.Instance.Initialized -= FadeOut;
			CreateTween()
				.SetTrans(Tween.TransitionType.Linear)
				.SetEase(Tween.EaseType.InOut)
				.TweenProperty(this, "color", new Color(0f, 0f, 0f, 0f), fadeDuration)
				.Finished += OnFadeOutFinished;
		}

		protected void OnFadeOutFinished()
		{
			Visible = false;
		}
	}
}
