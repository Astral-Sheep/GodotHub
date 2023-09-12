using Com.Astral.GodotHub.Settings;
using Godot;
using System;

namespace Com.Astral.GodotHub.Debug
{
	public sealed partial class Debugger : Control
	{
		public static bool Enabled
		{
			get => instance.Visible;
			set
			{
				if (instance.Visible == value)
					return;

				instance.Visible = value;
			}
		}

		private static Debugger instance;

		[ExportGroup("Parameters")]
		[Export] private RichTextLabel label;
		[ExportSubgroup("Colors")]
		[Export] private Color normalColor = new Color(184f / 255f, 184f / 255f, 184f / 255f);
		[Export] private Color validationColor = new Color(118f / 255f, 150f / 255f, 86f / 255f);
		[Export] private Color warningColor = new Color(231f / 255f, 180f / 255f, 79f / 255f);
		[Export] private Color errorColor = new Color(212f / 255f, 80f / 255f, 79f / 255f);

		private Debugger() : base()
		{
			if (instance != null)
			{
				GD.PushWarning($"{nameof(Debugger)} instance already exists, destroying the last added.");
				Free();
				return;
			}

			instance = this;
		}

		public override void _Ready()
		{
			if (!label.BbcodeEnabled)
			{
				label.BbcodeEnabled = true;
			}

			Enabled = Config.Debug;
		}

		protected override void Dispose(bool pDisposing)
		{
			if (pDisposing && instance == this)
			{
				instance = null;
			}
		}

		public static void PrintMessage(string pMessage)
		{
			instance.label.Text += FormatMessage(pMessage, instance.normalColor);
		}

		public static void PrintValidation(string pMessage)
		{
			instance.label.Text += FormatMessage(pMessage, instance.validationColor);
		}

		public static void PrintWarning(string pMessage)
		{
			instance.label.Text += FormatMessage(pMessage, instance.warningColor);
		}

		public static void PrintError(string pMessage)
		{
			instance.label.Text += FormatMessage($"[b]{pMessage}[/b]", instance.errorColor);
		}

		public static void PrintException(Exception pException)
		{
			PrintError($"{pException.GetType()}: {pException.Message}");
		}

		private static string FormatMessage(string pMessage, Color pColor)
		{
			return $"[color=#{pColor.R8:x2}{pColor.G8:x2}{pColor.B8:x2}]{pMessage}[/color]\n";
		}
	}
}
