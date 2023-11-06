using System.Text.RegularExpressions;

namespace Com.Astral.GodotHub.AdminInstall
{
	internal class Program
	{
		private static readonly Regex optionFormat = new Regex(@"--([a-z]+)");

		//arg[0] = --mode (--zip --exe --all)
		//arg[1] = --os (--win --unix --macos)
		//arg[2] = path_1
		//arg[3] = path_2 (optional) -> set to path_1 if null otherwise corresponds to exe path
		public static void Main(string[] pArgs)
		{
			if (pArgs.Length < 3)
			{
				Console.WriteLine("Less than 3 arguments");
				Console.ReadKey();
				throw new ArgumentException($"Missing arguments: only {pArgs.Length} given instead of 3 minimum");
			}

			Match lMatch = optionFormat.Match(pArgs[0]);

			if (!lMatch.Success)
			{
				Console.WriteLine($"Wrong format: {pArgs[0]}");
				Console.ReadKey();
				throw new ArgumentException($"Invalid OS format given: {pArgs[0]}");
			}

			string lArg = lMatch.Groups[1].Value.ToLower();

			switch (lArg)
			{
				case "zip":
					Console.WriteLine("Download zip");
					break;
				case "exe":
					if (pArgs.Length < 4)
					{
						Console.Write("Missing second path for ");
					}
					Console.WriteLine("Install zip");
					break;
				case "all":
					if (pArgs.Length < 4)
					{
						Console.Write("Missing second path for ");
					}
					Console.WriteLine("Download & Install zip");
					break;
				default:
					Console.WriteLine($"Invalid: {lArg}");
					break;
					//throw new ArgumentException($"Invalid mode given: {lArg}");
			}

			lMatch = optionFormat.Match(pArgs[1]);

			if (!lMatch.Success)
			{
				Console.WriteLine($"Wrong format: {pArgs[1]}");
				Console.ReadKey();
				throw new ArgumentException($"Invalid mode format given: {pArgs[1]}");
			}

			lArg = lMatch.Groups[1].Value.ToLower();

			switch (lArg)
			{
				case "win":
					Console.WriteLine("Windows");
					break;
				case "unix":
					Console.WriteLine("Linux");
					break;
				case "macos":
					Console.WriteLine("Macos");
					break;
				default:
					Console.WriteLine($"Invalid: {lArg}");
					break;
					//throw new ArgumentException($"Invalid OS given: {lArg}");
			}

			Console.WriteLine(pArgs[2]);

			if (pArgs.Length >= 4)
			{
				Console.WriteLine(pArgs[3]);
			}

			Console.ReadKey();
		}

		private static void WriteZip(string pZipPath)
		{

		}

		private static void UnzipWindows(string pZipPath, string pExePath)
		{

		}

		private static void UnzipLinux(string pZipPath, string pExePath)
		{

		}

		private static void UnzipMacos(string pZipPath, string pExePath)
		{

		}
	}
}
