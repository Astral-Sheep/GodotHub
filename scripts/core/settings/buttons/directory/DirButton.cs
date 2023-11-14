using Godot;

namespace Com.Astral.GodotHub.Core.Settings.Buttons.Directory
{
	public abstract partial class DirButton : SettingButton
	{
		public string Text
		{
			get => button.Text;
			set
			{
				button.Text = value;
			}
		}

		[Export] protected PackedScene folderDialogScene;

		public override void Connect()
		{
			button.Pressed += OnPressed;
		}

		public override void Disconnect()
		{
			button.Pressed -= OnPressed;
		}

		protected virtual void OnPressed()
		{
			button.Pressed -= OnPressed;
			FileDialog lDialog = folderDialogScene.Instantiate<FileDialog>();
			Main.Instance.AddChild(lDialog);
			lDialog.PopupCentered();
			lDialog.RootSubfolder = "";
			lDialog.CurrentDir = button.Text[1..];
			lDialog.DirSelected += OnDirSelected;
			lDialog.Canceled += OnCanceled;
		}

		protected virtual void OnDirSelected(string pDir)
		{
			button.Text = $" {pDir}";
			button.Pressed += OnPressed;
		}

		protected void OnCanceled()
		{
			button.Pressed += OnPressed;
		}
	}
}
