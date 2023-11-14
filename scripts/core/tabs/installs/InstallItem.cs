using Com.Astral.GodotHub.Core.Data;
using Com.Astral.GodotHub.Core.Debug;
using Com.Astral.GodotHub.Core.Utils;
using Com.Astral.GodotHub.Core.Utils.Comparisons;
using Godot;
using System;
using System.Diagnostics;
using System.IO;

using Colors = Com.Astral.GodotHub.Core.Utils.Colors;
using Debugger = Com.Astral.GodotHub.Core.Debug.Debugger;
using Label = Godot.Label;
using Version = Com.Astral.GodotHub.Core.Data.Version;

namespace Com.Astral.GodotHub.Core.Tabs.Installs
{
	public partial class InstallItem : Control, IFavoriteItem, IMonoItem, ITimedItem, IVersionItem, IValidItem
	{
		/// <summary>
		/// Event called when <see cref="Close"/> has been called and the item is going to be disposed
		/// </summary>
		public static event Action<InstallItem> Closed;

		public bool IsFavorite { get; protected set; }
		public bool IsMono { get; protected set; }
		public bool IsValid { get; protected set; } = true;
		public double TimeSinceLastOpening { get; protected set; }
		public Version Version => install.Version;

		[Export] protected PackedScene confirmationPopup;
		[Export] protected float closeDuration = 0.25f;

		[ExportGroup("Data")]
		[Export] protected RichTextLabel nameLabel;
		[Export] protected Label pathLabel;
		[Export] protected CheckBox isMonoBox;
		[Export] protected RichTextLabel timeLabel;

		[ExportGroup("Buttons")]
		[Export] protected Button favoriteToggle;
		[Export] protected Button openButton;
		[Export] protected Button uninstallButton;

		protected GDFile install;

		/// <summary>
		/// Set the data of this <see cref="InstallItem"/>
		/// </summary>
		public void Init(GDFile pInstall)
		{
			install = pInstall;
			pathLabel.Text = install.Path;
			favoriteToggle.ButtonPressed = install.IsFavorite;
			favoriteToggle.Toggled += OnFavoriteToggled;
			IsFavorite = install.IsFavorite;

			if (!File.Exists(install.Path))
			{
				nameLabel.Text = $"[color=#{Colors.ToHexa(Colors.Singleton.Red)}][b]Missing version[/b][/color]";
				isMonoBox.ButtonPressed = false;
				timeLabel.Text = $"[color=#{Colors.ToHexa(Colors.Singleton.Red)}]N/A[/color]";
				openButton.Disabled = true;
				uninstallButton.Text = "Remove";
				uninstallButton.Pressed += OnRemovePressed;
				IsValid = false;
				return;
			}

			nameLabel.Text = $"[b]Godot {install.Version}[/b]";
			isMonoBox.ButtonPressed = install.Path.Contains("mono");
			IsMono = isMonoBox.ButtonPressed;
			DateTime lTime = new FileInfo(install.Path).LastAccessTimeUtc;
			TimeSinceLastOpening = (DateTime.UtcNow - lTime).TotalSeconds;
			timeLabel.Text = TimeFormater.Format(lTime);
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
				ExceptionHandler.Singleton.LogException(lException);
				//Debugger.LogException(lException);
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
				ExceptionHandler.Singleton.LogException(lException);
				//Debugger.LogException(lException);
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
			IsFavorite = pToggled;
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
