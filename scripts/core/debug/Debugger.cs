using Com.Astral.GodotHub.Core.Data;
using Godot;
using System;
using Com.Astral.GodotHub.Core.Utils;
using Colors = Com.Astral.GodotHub.Core.Utils.Colors;

namespace Com.Astral.GodotHub.Core.Debug
{
	/// <summary>
	/// In-app console
	/// </summary>
	public sealed partial class Debugger : Control
	{
		/// <summary>
		/// Whether or not the console is visible
		/// </summary>
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

			Enabled = AppConfig.Debug;
		}

		protected override void Dispose(bool pDisposing)
		{
			if (!pDisposing)
				return;

			if (instance == this)
			{
				instance = null;
			}
		}

		/// <summary>
		/// Print an <see cref="object"/> in <see cref="Colors.White"/>
		/// </summary>
		public static void LogObject(object pObject)
		{
			LogMessage(pObject.ToString());
		}

		/// <summary>
		/// Print a message in <see cref="Colors.White"/>
		/// </summary>
		public static void LogMessage(string pMessage)
		{
			instance.label.Text += FormatMessage(pMessage, Colors.Singleton.White);
		}

		/// <summary>
		/// Print a message in <see cref="Colors.Green"/>
		/// </summary>
		public static void LogValidation(string pMessage)
		{
			instance.label.Text += FormatMessage(pMessage, Colors.Singleton.Green);
		}

		/// <summary>
		/// Print a message in <see cref="Colors.Yellow"/>
		/// </summary>
		public static void LogWarning(string pMessage)
		{
			instance.label.Text += FormatMessage(pMessage, Colors.Singleton.Yellow);
		}

		/// <summary>
		/// Print a message in bold <see cref="Colors.Red"/>
		/// </summary>
		public static void LogError(string pMessage)
		{
			instance.label.Text += FormatMessage($"[b]{pMessage}[/b]", Colors.Singleton.Red);
		}

		/// <summary>
		/// Print an <see cref="Exception"/> in bold <see cref="Colors.Red"/>
		/// </summary>
		public static void LogException(Exception pException)
		{
			LogError($"{pException.GetType()}: {pException.Message}");
#if DEBUG
			GD.PrintErr(pException);
#endif //DEBUG
		}

		private static string FormatMessage(string pMessage, Color pColor)
		{
			return BBCodeT.GetColoredText(pMessage, pColor) + '\n';
		}
	}
}
