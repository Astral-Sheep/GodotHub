using Com.Astral.GodotHub.Core.Data;
using Com.Astral.GodotHub.Core.Debug;
using Godot;
using System;

using Error = Com.Astral.GodotHub.Core.Utils.Error;
using GError = Godot.Error;

namespace Com.Astral.GodotHub.Core
{
	public partial class Main : Node
	{
		private const string CURRENT_VERSION = "0.1.4";
		private static readonly Vector2I DefaultWindowSize = new Vector2I(1100, 600);
		
		public static Main Instance { get; private set; }

		public event Action Initialized;

		[Export] protected PackedScene fileDialogScene;

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

		public override void _EnterTree()
		{
			DisplayServer.WindowSetMinSize(DefaultWindowSize);

			// ConfigFile lExportPresets = new ConfigFile();
			// lExportPresets.Load("res://export_presets.cfg");
			// string lAppVersion = (string)lExportPresets.GetValue("preset.0.options", "application/product_version", "0.0.0.0");
			// lAppVersion = lAppVersion[..lAppVersion.RFind(".")];
			
			DisplayServer.WindowSetTitle(
				$"{ProjectSettings.GetSetting("application/config/name", "Godot Hub")}" +
				" " +
				$"{GetProductVersion()}"
			);
		}

		public override void _Ready()
		{
			Init();
		}

		protected override void Dispose(bool pDisposing)
		{
			if (!pDisposing)
				return;

			if (Instance == this)
			{
				Instance = null;
			}
		}

		protected async void Init()
		{
			Error lError = await GDRepository.Init();

			if (!lError.Ok)
			{
				ExceptionHandler.Singleton.LogException(lError.Exception);
			}

			Initialized?.Invoke();
		}

		protected string GetProductVersion()
		{
			ConfigFile lExportPresets = new ConfigFile();
			GError lError = lExportPresets.Load("res://export_presets.cfg");

			if (lError != GError.Ok)
			{
				return "0.0.0";
			}

#if GODOT_WINDOWS
			int lPreset = 0;
#elif GODOT_LINUXBSD
			int lPreset = 1;
#else
			int lPreset = 2;
#endif

			string lVersion = (string)lExportPresets.GetValue($"preset.{lPreset}.options", "application/product_version", "0.0.0.0");
			return lVersion[..lVersion.RFind(".")];
		}

		public FileDialog InstantiateFileDialog(Node pParent = null, bool pAutoFree = true)
		{
			FileDialog lDialog = fileDialogScene.Instantiate<FileDialog>();

			if (pParent == null)
			{
				AddChild(lDialog);
			}
			else
			{
				pParent.AddChild(lDialog);
			}

			lDialog.GetCancelButton().FocusMode = Control.FocusModeEnum.None;
			lDialog.GetOkButton().FocusMode = Control.FocusModeEnum.None;
			lDialog.PopupCentered();

			if (pAutoFree)
			{
				lDialog.CloseRequested += () => lDialog.QueueFree();
			}

			return lDialog;
		}
	}
}
