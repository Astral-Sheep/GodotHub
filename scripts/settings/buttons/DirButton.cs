using Godot;

namespace Com.Astral.GodotHub.Settings.Buttons
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
			lDialog.CurrentDir = button.Text;
			lDialog.DirSelected += OnDirSelected;
			Main.Instance.AddChild(lDialog);
		}

		protected virtual void OnDirSelected(string pDir)
		{
			button.Text = " " + pDir;
			button.Pressed += OnPressed;
		}
	}
}
