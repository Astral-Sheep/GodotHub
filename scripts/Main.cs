using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Debug;
using Godot;
using System;

using Error = Com.Astral.GodotHub.Utils.Error;

namespace Com.Astral.GodotHub
{
	public partial class Main : Node
	{
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

		public override void _Ready()
		{
			//Hardcoded values because they're completely arbitrary
			DisplayServer.WindowSetMinSize(new Vector2I(1100, 600));
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
			Error lError = await GDRepository.Init();

			if (!lError.Ok)
			{
				Debugger.PrintException(lError.Exception);
			}

			Initialized?.Invoke();
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
				//lDialog.CloseRequested += () => lDialog.QueueFree();
			}

			return lDialog;
		}
	}
}
