using Com.Astral.GodotHub.Settings.Buttons;
using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace Com.Astral.GodotHub.Settings
{
	public partial class SettingsPanel : Control
	{
		[Export] protected Control content;
		[ExportGroup("Buttons")]
		[Export] protected Button openButton;
		[Export] protected Button closeButton;
		[Export] protected Button backgroundButton;
		[ExportSubgroup("Settings buttons")]
		[Export] protected Array<NodePath> buttonsPath;
		protected List<IConnecter> buttons;

		public override void _Ready()
		{
			buttons = new List<IConnecter>();

			for (int i = 0; i < buttonsPath.Count; i++)
			{
				buttons.Add(GetNode<IConnecter>(buttonsPath[i]));
			}

			Resized += OnResized;
			CloseInstant();
			openButton.Pressed += OpenPressed;
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
			Tween lTween = CreateTween()
				.SetParallel()
				.SetTrans(Tween.TransitionType.Cubic)
				.SetEase(Tween.EaseType.Out);
			lTween.TweenProperty(content, "position", new Vector2(content.Position.X, 0), 0.2f);
			lTween.TweenProperty(backgroundButton, "self_modulate", new Color(backgroundButton.SelfModulate, 1f), 0.2f);
			
			EnableButtons();
			openButton.Pressed -= OpenPressed;
			closeButton.Pressed += ClosePressed;
			backgroundButton.Pressed += ClosePressed;
			backgroundButton.MouseFilter = MouseFilterEnum.Stop;
		}

		protected void ClosePressed()
		{
			Config.Save();

			backgroundButton.MouseFilter = MouseFilterEnum.Ignore;
			backgroundButton.Pressed -= ClosePressed;
			closeButton.Pressed -= ClosePressed;
			openButton.Pressed += OpenPressed;
			DisableButtons();

			Tween lTween = CreateTween()
				.SetParallel()
				.SetTrans(Tween.TransitionType.Cubic)
				.SetEase(Tween.EaseType.In);
			lTween.TweenProperty(content, "position", new Vector2(content.Position.X, -content.Size.Y), 0.2f);
			lTween.TweenProperty(backgroundButton, "self_modulate", new Color(backgroundButton.SelfModulate, 0f), 0.2f);
		}

		protected void CloseInstant()
		{
			content.Position = new Vector2(content.Position.X, -content.Size.Y);
			backgroundButton.SelfModulate = new Color(backgroundButton.SelfModulate, 0f);
			backgroundButton.MouseFilter = MouseFilterEnum.Ignore;
		}

		protected void OnResized()
		{
			content.Position = new Vector2(
				content.Position.X,
				content.Position.Y == 0 ? content.Position.Y : -content.Size.Y
			);
		}

		protected void EnableButtons()
		{
			for (int i = 0; i < buttons.Count; i++)
			{
				buttons[i].Connect();
			}
		}

		protected void DisableButtons()
		{
			for (int i = 0; i < buttons.Count; i++)
			{
				buttons[i].Disconnect();
			}
		}
	}
}
