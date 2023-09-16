using Com.Astral.GodotHub.Data;
using Godot;
using System;
using System.Diagnostics;
using System.IO;

using Debugger = Com.Astral.GodotHub.Debug.Debugger;
using Version = Com.Astral.GodotHub.Data.Version;

namespace Com.Astral.GodotHub.Tabs.Installs
{
	public partial class InstallItem : Control
	{
		public static event Action<InstallItem> Closed;

		public Version Version => install.Version;
		public int Index { get; protected set; }

		[Export] protected PackedScene confirmationPopup;
		[Export] protected float closeDuration = 0.25f;

		[ExportGroup("Data")]
		[Export] protected RichTextLabel nameLabel;
		[Export] protected Label pathLabel;
		[Export] protected CheckBox isMonoBox;

		[ExportGroup("Buttons")]
		[Export] protected Button favoriteToggle;
		[Export] protected Button openButton;
		[Export] protected Button uninstallButton;

		protected GDFile install;

		public void Init(GDFile pInstall, int pIndex)
		{
			Index = pIndex;
			install = pInstall;
			pathLabel.Text = install.Path;
			favoriteToggle.ButtonPressed = install.IsFavorite;

			if (!File.Exists(install.Path))
			{
				nameLabel.Text = $"[color=#{Colors.ToHexa(Colors.Singleton.Red)}][b]Missing version[/b][/color]";
				isMonoBox.ButtonPressed = false;
				openButton.Disabled = true;
				uninstallButton.Text = "Remove";
				uninstallButton.Pressed += OnRemovePressed;
				return;
			}

			nameLabel.Text = $"[b]Godot {install.Version}[/b]";
			isMonoBox.ButtonPressed = install.Path.Contains("mono");
			openButton.Pressed += Open;
			uninstallButton.Pressed += OnUninstallPressed;
		}

		protected void Open()
		{
			try
			{
				Process.Start(new ProcessStartInfo() {
					FileName = pathLabel.Text,
					WorkingDirectory = pathLabel.Text[..pathLabel.Text.RFind("/")],
					Arguments = "--project-manager"
				});
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
			}
		}

		protected void Uninstall()
		{
			try
			{
				Directory.Delete(PathT.GetFolderFromExe(pathLabel.Text), true);
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
			}

			Remove();
		}

		protected void Remove()
		{
			InstallsData.RemoveVersion(install.Version);
			Close();
		}

		protected void Close()
		{
			CreateTween()
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.Out)
				.TweenProperty(this, "custom_minimum_size:y", 0f, closeDuration)
				.Finished += QueueFree;
			Closed?.Invoke(this);
		}

		protected ConfirmationDialog CreateDialog(string pTitle, string pText)
		{
			ConfirmationDialog lDialog = confirmationPopup.Instantiate<ConfirmationDialog>();
			Main.Instance.AddChild(lDialog);
			lDialog.Title = pTitle;
			lDialog.DialogText = pText;
			lDialog.GetChild<Label>(1, true).HorizontalAlignment = HorizontalAlignment.Center;
			lDialog.PopupCentered();
			return lDialog;
		}

		protected void OnFavoriteToggled(bool pToggled)
		{
			InstallsData.SetFavorite(Version, pToggled);
		}

		protected void OnUninstallPressed()
		{
			ConfirmationDialog lDialog = CreateDialog(
				"Uninstall",
				"Do you really want to uninstall this version?"
			);
			lDialog.Confirmed += Uninstall;
		}

		protected void OnRemovePressed()
		{
			ConfirmationDialog lDialog = CreateDialog(
				"Remove",
				"Do you really want to remove this version?"
			);
			lDialog.Confirmed += Remove;
		}
	}
}
