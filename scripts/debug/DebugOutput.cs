using Godot;
using Godot.Collections;

namespace Com.Astral.GodotHub.Debug
{
	public partial class DebugOutput : RichTextLabel
	{
		public static DebugOutput Instance { get; private set; }
		private static Dictionary<int, char> DecimalToHexa = new Dictionary<int, char>() {
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

		[ExportGroup("Debug colors")]
		[Export] protected Color normalColor = new Color(1f, 1f, 1f);
		[Export] protected Color validationColor = new Color(0f, 1f, 0f);
		[Export] protected Color warningColor = new Color(1f, 1f, 0f);
		[Export] protected Color errorColor = new Color(1f, 0f, 0f);

		private DebugOutput() : base()
		{
			if (Instance != null)
			{
				GD.PushWarning($"{nameof(DebugOutput)} instance already exists, destroying the last added.");
				Free();
				return;
			}

			Instance = this;

			if (!BbcodeEnabled)
			{
				BbcodeEnabled = true;
			}
		}

		public void PrintMessage(string pMessage)
		{
			Text = FormatMessage(pMessage, normalColor);
		}

		public void PrintValidation(string pMessage)
		{
			Text = FormatMessage(pMessage, validationColor);
		}

		public void PrintWarning(string pMessage)
		{
			Text = FormatMessage(pMessage, warningColor);
		}

		public void PrintError(string pMessage)
		{
			Text = FormatMessage("[b]" + pMessage + "[/b]", errorColor);
		}

		private static string FormatMessage(string pMessage, Color pColor)
		{
			return $"[color=#{RGBToHexa(pColor)}]{pMessage}[/color]";
		}

		private static string RGBToHexa(Color pColor)
		{
			return DecToHexa(pColor.R8) + DecToHexa(pColor.G8) + DecToHexa(pColor.B8);
		}

		private static string DecToHexa(int pComponent)
		{
			return $"{DecimalToHexa[pComponent / 16]}{DecimalToHexa[pComponent % 16]}";
		}
	}
}
