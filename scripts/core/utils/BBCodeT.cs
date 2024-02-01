using Godot;

namespace Com.Astral.GodotHub.Core.Utils
{
	public static class BBCodeT
	{
		public static string GetColoredText(string pText, Color pColor)
		{
			return $"[color=#{Colors.ToHexa(pColor)}]{pText}[/color]";
		}

		public static string GetBoldText(string pText)
		{
			return $"[b]{pText}[/b]";
		}

		public static string GetItalicText(string pText)
		{
			return $"[i]{pText}[/i]";
		}

		public static string GetTaggedText(string pText, params string[] pTags)
		{
			string lRet = "";

			for (int i = 0; i < pTags.Length; i++)
			{
				lRet += "[" + pTags[i] + "]";
			}

			lRet += pText;

			for (int i = pTags.Length - 1; i >= 0; i--)
			{
				lRet += "[/" + pTags[i] + "]";
			}

			return lRet;
		}
	}
}
