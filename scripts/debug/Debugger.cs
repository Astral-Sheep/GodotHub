using Com.Astral.GodotHub.Settings;
using Godot;
using Godot.Collections;
using System;

namespace Com.Astral.GodotHub.Debug
{
	public sealed partial class Debugger : Control
	{
		public static bool Enabled
		{
			get => instance.enabled;
			set
			{
				if (instance.enabled == value)
					return;

				instance.enabled = value;
				instance.Visible = value;
			}
		}

		private static Debugger instance;
		private static readonly Dictionary<int, char> decimalToHexa = new Dictionary<int, char>() {
			{ 0, '0' },
			{ 1, '1' },
			{ 2, '2' },
			{ 3, '3' },
			{ 4, '4' },
			{ 5, '5' },
			{ 6, '6' },
			{ 7, '7' },
			{ 8, '8' },
			{ 9, '9' },
			{ 10, 'a' },
			{ 11, 'b' },
			{ 12, 'c' },
			{ 13, 'd' },
			{ 14, 'e' },
			{ 15, 'f' },
		};

		[Export] private bool enabled = true;

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

			enabled = Config.Debug;
			Visible = enabled;
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
			return $"[color=#{RGBToHexa(pColor)}]{pMessage}[/color]\n";
		}

		private static string RGBToHexa(Color pColor)
		{
			return DecToHexa(pColor.R8) + DecToHexa(pColor.G8) + DecToHexa(pColor.B8);
		}

		private static string DecToHexa(int pComponent)
		{
			return $"{decimalToHexa[pComponent / 16]}{decimalToHexa[pComponent % 16]}";
		}
	}
}
