using Godot;
using System;

using Colors = Com.Astral.GodotHub.Utils.Colors;

namespace Com.Astral.GodotHub.Debug
{
	public partial class ExceptionHandler : Node
	{
		public enum ExceptionGravity
		{
			Message,
			Warning,
			Error,
		}

		public static ExceptionHandler Singleton { get; private set; }

		[Export] protected PackedScene popupScene;
		[ExportGroup("Colors")]
		[Export] protected Color warningColor = new Color(1f, 1f, 0f);
		[Export] protected Color errorColor = new Color(1f, 0f, 0f);

		public override void _EnterTree()
		{
			Singleton = this;
		}

		public override void _ExitTree()
		{
			Singleton = null;
		}

		public void LogException(Exception pException, bool pLogOnConsole = true)
		{
			if (pLogOnConsole)
			{
				Debugger.LogException(pException);
			}

			LogMessage(pException.Message, pException.GetType().ToString(), ExceptionGravity.Error);
		}

		public void LogMessage(string pMessage, string pTitle = null, ExceptionGravity pGravity = ExceptionGravity.Message)
		{
			Color? lColor = pGravity switch {
				ExceptionGravity.Error => errorColor,
				ExceptionGravity.Warning => warningColor,
				_ => null
			};

			CreatePopup(pMessage, pTitle ?? pGravity.ToString(), lColor);
		}

		private void CreatePopup(string pMessage, string pTitle, Color? pColor = null)
		{
			AcceptDialog lPopup = popupScene.Instantiate<AcceptDialog>();
			AddChild(lPopup);

			if (pColor != null)
			{
				StyleBoxFlat lStyleBox = lPopup.GetThemeStylebox("panel").Duplicate() as StyleBoxFlat;
				lStyleBox.BorderColor = (Color)pColor;
				lPopup.AddThemeStyleboxOverride("panel", lStyleBox);
				lStyleBox = lPopup.GetThemeStylebox("embedded_border").Duplicate() as StyleBoxFlat;
				lStyleBox.BgColor = (Color)pColor;
				lPopup.AddThemeStyleboxOverride("embedded_border", lStyleBox);
			}

			lPopup.GetOkButton().FocusMode = Control.FocusModeEnum.None;
			lPopup.PopupCentered();
			lPopup.Title = pTitle;
			lPopup.DialogText = pMessage;
			lPopup.CloseRequested += () => lPopup.QueueFree();
		}
	}
}
