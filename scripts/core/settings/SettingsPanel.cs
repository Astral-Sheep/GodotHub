using Com.Astral.GodotHub.Core.Data;
using Com.Astral.GodotHub.Core.Settings.Buttons;
using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace Com.Astral.GodotHub.Core.Settings
{
	public partial class SettingsPanel : Control
	{
		[Export] protected Control content;
		[Export] protected float openDuration = 0.2f;

		[ExportGroup("Buttons")]
		[Export] protected Button openButton;
		[Export] protected Button closeButton;
		[Export] protected Button backgroundButton;
		[ExportSubgroup("Settings buttons")]
		[Export] protected Array<NodePath> buttonsPath;

		protected List<ISettingButton> buttons;
		protected (float top, float bottom) anchor;

		public override void _Ready()
		{
#if DEBUG
			Visible = true;
#endif //DEBUG

			buttons = new List<ISettingButton>();

			for (int i = 0; i < buttonsPath.Count; i++)
			{
				buttons.Add(GetNode<ISettingButton>(buttonsPath[i]));
			}

			anchor = (content.AnchorTop, content.AnchorBottom);
			CloseInstant();
			openButton.Pressed += OnOpenPressed;
		}

		protected override void Dispose(bool pDisposing)
		{
			if (!pDisposing)
				return;

			if (openButton != null)
			{
				openButton.Pressed -= OnOpenPressed;
			}
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

		protected void OnOpenPressed()
		{
			Tween lTween = CreateTween()
				.SetParallel()
				.SetTrans(Tween.TransitionType.Cubic)
				.SetEase(Tween.EaseType.Out);
			lTween.TweenProperty(content, "anchor_top", anchor.top,openDuration);
			lTween.TweenProperty(content, "anchor_bottom", anchor.bottom, openDuration);
			lTween.TweenProperty(backgroundButton, "self_modulate", new Color(backgroundButton.SelfModulate, 1f), openDuration);
			
			EnableButtons();
			openButton.Pressed -= OnOpenPressed;
			closeButton.Pressed += OnClosePressed;
			backgroundButton.Pressed += OnClosePressed;
			backgroundButton.MouseFilter = MouseFilterEnum.Stop;
		}

		protected void OnClosePressed()
		{
			AppConfig.Save();

			backgroundButton.MouseFilter = MouseFilterEnum.Ignore;
			backgroundButton.Pressed -= OnClosePressed;
			closeButton.Pressed -= OnClosePressed;
			openButton.Pressed += OnOpenPressed;
			DisableButtons();

			Tween lTween = CreateTween()
				.SetParallel()
				.SetTrans(Tween.TransitionType.Cubic)
				.SetEase(Tween.EaseType.In);
			lTween.TweenProperty(content, "anchor_top", -anchor.bottom, openDuration);
			lTween.TweenProperty(content, "anchor_bottom", anchor.top, openDuration);
			lTween.TweenProperty(backgroundButton, "self_modulate", new Color(backgroundButton.SelfModulate, 0f), openDuration);
		}

		protected void CloseInstant()
		{
			content.AnchorTop = -anchor.bottom;
			content.AnchorBottom = anchor.top;
			backgroundButton.SelfModulate = new Color(backgroundButton.SelfModulate, 0f);
			backgroundButton.MouseFilter = MouseFilterEnum.Ignore;
		}
	}
}
