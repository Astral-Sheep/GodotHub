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
			instance.label.Text += FormatMessage(pMessage, Colors.Singleton.White);
		}

		public static void PrintValidation(string pMessage)
		{
			instance.label.Text += FormatMessage(pMessage, Colors.Singleton.Green);
		}

		public static void PrintWarning(string pMessage)
		{
			instance.label.Text += FormatMessage(pMessage, Colors.Singleton.Yellow);
		}

		public static void PrintError(string pMessage)
		{
			instance.label.Text += FormatMessage($"[b]{pMessage}[/b]", Colors.Singleton.Red);
		}

		public static void PrintException(Exception pException)
		{
			PrintError($"{pException.GetType()}: {pException.Message}");
		}

		private static string FormatMessage(string pMessage, Color pColor)
		{
			return $"[color=#{Colors.ToHexa(pColor)}]{pMessage}[/color]\n";
		}
	}
}
