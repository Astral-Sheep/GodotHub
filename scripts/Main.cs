using Com.Astral.GodotHub.Debug;
using Com.Astral.GodotHub.Releases;
using Godot;

namespace Com.Astral.GodotHub
{
	public partial class Main : Node
	{
		public override void _Ready()
		{
			Init();
		}

		protected async void Init()
		{
			if (await GodotRepo.Init())
			{
				Debugger.PrintMessage("Github client initiated");
			}

			const string VERSION = "4.0.4";

			switch (await VersionInstaller.InstallVersion($"{VERSION}-stable", "win64", true))
			{
				case VersionInstaller.InstallationResult.Installed:
					Debugger.PrintValidation($"Godot {VERSION} installed successfully");
					break;
				case VersionInstaller.InstallationResult.Downloaded:
					Debugger.PrintWarning($"Godot {VERSION} downloaded but failed to install");
					break;
				case VersionInstaller.InstallationResult.Failed:
					Debugger.PrintError($"Failed to download Godot {VERSION}");
					break;
			}
		}
	}
}
