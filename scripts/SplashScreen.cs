using Godot;

namespace Com.Astral.GodotHub
{
	public partial class SplashScreen : ColorRect
	{
		[Export] protected float fadeDuration = 0.2f;

		public override void _Ready()
		{
#if DEBUG
			Visible = true;
#endif //DEBUG

			Main.Instance.Initialized += FadeOut;
		}

		protected void FadeOut()
		{
			Main.Instance.Initialized -= FadeOut;
			CreateTween()
				.SetTrans(Tween.TransitionType.Linear)
				.SetEase(Tween.EaseType.InOut)
				.TweenProperty(this, "self_modulate:a", 0f, fadeDuration)
				.Finished += OnFadeOutFinished;
		}

		protected void OnFadeOutFinished()
		{
			Visible = false;
		}
	}
}
