using Com.Astral.GodotHub.Debug;
using Com.Astral.GodotHub.Releases;
using Godot;

namespace Com.Astral.GodotHub
{
	public partial class Main : Node
	{
		public static Main Instance { get; private set; }

		private Main() : base()
		{
			if (Instance != null)
			{
				GD.PushWarning($"{nameof(Main)} instance already exists, destroying the last added");
				Free();
				return;
			}

			Instance = this;
		}

		public override void _Ready()
		{
			Init();
		}

		protected override void Dispose(bool pDisposing)
		{
			if (pDisposing && Instance == this)
			{
				Instance = null;
			}
		}

		protected async void Init()
		{
			await GodotRepo.Init();

			//const string VERSION = "4.0.4";

			//switch (await VersionInstaller.InstallVersion($"{VERSION}-stable", "win64", true))
			//{
			//	case VersionInstaller.InstallationResult.Installed:
			//		Debugger.PrintValidation($"Godot {VERSION} installed successfully");
			//		break;
			//	case VersionInstaller.InstallationResult.Downloaded:
			//		Debugger.PrintWarning($"Godot {VERSION} downloaded but failed to install");
			//		break;
			//	case VersionInstaller.InstallationResult.Failed:
			//		Debugger.PrintError($"Failed to download Godot {VERSION}");
			//		break;
			//	case VersionInstaller.InstallationResult.Cancelled:
			//		Debugger.PrintWarning($"Godot {VERSION} already installed");
			//		break;
			//}
		}
	}
}
