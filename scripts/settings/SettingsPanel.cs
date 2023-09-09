using Com.Astral.GodotHub.Debug;
using Godot;

namespace Com.Astral.GodotHub.Settings
{
	public partial class SettingsPanel : Control
	{
		[Export] protected Control content;
		[ExportGroup("Buttons")]
		[Export] protected Button openButton;
		[Export] protected Button closeButton;
		[Export] protected Button backgroundButton;
		[Export] protected Button debugToggle;

		public override void _Ready()
		{
			debugToggle.ButtonPressed = Debugger.Enabled;
			CloseInstant();
			openButton.Pressed += OpenPressed;
			debugToggle.Toggled += DebugToggled;
		}

		protected override void Dispose(bool pDisposing)
		{
			if (!pDisposing)
				return;

			if (openButton != null)
			{
				openButton.Pressed -= OpenPressed;
			}
		}

		protected void OpenPressed()
		{
			Tween lTween = CreateTween().SetParallel().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
			lTween.TweenProperty(content, "position", new Vector2(content.Position.X, 0), 0.1f);
			lTween.TweenProperty(backgroundButton, "self_modulate", new Color(backgroundButton.SelfModulate, 1f), 0.1f);

			openButton.Pressed -= OpenPressed;
			closeButton.Pressed += ClosePressed;
			backgroundButton.Pressed += ClosePressed;
			backgroundButton.MouseFilter = MouseFilterEnum.Stop;
		}

		protected void ClosePressed()
		{
			Tween lTween = CreateTween()
				.SetParallel()
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.In);
			lTween.TweenProperty(content, "position", new Vector2(content.Position.X, -content.Size.Y), 0.1f);
			lTween.TweenProperty(backgroundButton, "self_modulate", new Color(backgroundButton.SelfModulate, 0f), 0.1f);

			backgroundButton.MouseFilter = MouseFilterEnum.Ignore;
			backgroundButton.Pressed -= ClosePressed;
			closeButton.Pressed -= ClosePressed;
			openButton.Pressed += OpenPressed;
		}

		protected void CloseInstant()
		{
			content.Position = new Vector2(content.Position.X, -content.Size.Y);
			backgroundButton.SelfModulate = new Color(backgroundButton.SelfModulate, 0f);
			backgroundButton.MouseFilter = MouseFilterEnum.Ignore;
		}

		protected void DebugToggled(bool pToggled)
		{
			Debugger.Enabled = pToggled;
		}
	}
}
