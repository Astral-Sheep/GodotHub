using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Debug;
using Godot;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;

using Label = Godot.Label;
using OS = Com.Astral.GodotHub.Data.OS;
using Version = Com.Astral.GodotHub.Data.Version;

namespace Com.Astral.GodotHub.Tabs.Installs
{
	public partial class ReleaseItem : Control
	{
		protected const string RELEASE_NAME_SUFFIX = "-stable";
		protected const string ASSET_NAME_PREFIX = "Godot_v";
		protected const string FILE_TYPE = ".zip";

		public static event Action<ReleaseItem, Source> InstallClicked;

		public int Index { get; protected set; }
		public Version Version { get; protected set; }

		[Export] protected Label versionLabel;
		[Export] protected Label dateLabel;
		[Export] protected CheckBox monoCheck;
		[Export] protected OptionButton osButton;
		[Export] protected OptionButton architectureButton;
		[Export] protected Button installButton;

		protected List<Source> sources;
		protected List<Installer> installers;
		protected string assetNamePrefix;

		protected string assetName = "";
		protected bool nameIsValid = false;

		public override void _Ready()
		{
			AddItem(osButton, OS.Windows);
			AddItem(osButton, OS.Linux);
			AddItem(osButton, OS.MacOS);
			AddItem(architectureButton, Architecture.x32);
			AddItem(architectureButton, Architecture.x64);

			osButton.GetPopup().TransparentBg = true;
			architectureButton.GetPopup().TransparentBg = true;

			monoCheck.Toggled += OnMonoChanged;
			osButton.ItemSelected += OnOsChanged;
			architectureButton.ItemSelected += OnArchitectureChanged;
			installButton.Pressed += OnInstallClicked;

			osButton.Selected = (int)Config.os;
			architectureButton.Selected = (int)Config.architecture;
		}

		public void Init(Release pRelease, int pIndex)
		{
			Index = pIndex;
			assetNamePrefix = $"{ASSET_NAME_PREFIX}{pRelease.Name}";
			string lVersion = pRelease.Name[0..pRelease.Name.Find(RELEASE_NAME_SUFFIX)];
			Version = Source.GetVersion(pRelease);

			installers = new List<Installer>();
			sources = new List<Source>();
			ReleaseAsset lReleaseAsset;

			for (int i = 0; i < pRelease.Assets.Count; i++)
			{
				lReleaseAsset = pRelease.Assets[i];

				if (lReleaseAsset.Name[0] != ASSET_NAME_PREFIX[0] || lReleaseAsset.Name[^4..^0] != FILE_TYPE)
					continue;

				sources.Add(GetSource(lReleaseAsset));
			}

			versionLabel.Text = $"Godot {lVersion}";
			dateLabel.Text = $"{pRelease.CreatedAt.Day:D2}/{pRelease.CreatedAt.Month:D2}/{pRelease.CreatedAt.Year:D4}";
			SetInstallButton();
		}

		#region EVENT_HANDLING

		protected void OnMonoChanged(bool _)
		{
			nameIsValid = false;
			SetInstallButton();
		}

		protected void OnOsChanged(long pOS)
		{
			nameIsValid = false;
			architectureButton.Visible = pOS != (long)OS.MacOS;
			SetInstallButton();
		}

		protected void OnArchitectureChanged(long _)
		{
			nameIsValid = false;
			SetInstallButton();
		}

		protected void OnInstallClicked()
		{
			string lAssetName = GetAssetName(true);

			foreach (Source source in sources)
			{
				if (source.asset.Name == lAssetName)
				{
					SetInstalling();
					InstallClicked?.Invoke(this, source);
					return;
				}
			}

			Debugger.PrintError($"No asset named {lAssetName}");
		}

		#endregion //EVENT_HANDLING

		#region INSTALLATION

		public void Connect(Installer pInstaller)
		{
			if (installers.Contains(pInstaller))
				return;

			installers.Add(pInstaller);
			pInstaller.Completed += OnInstallationCompleted;
		}

		protected void OnInstallationCompleted(Installer pInstaller, Installer.Result pResult)
		{
			if (!installers.Contains(pInstaller))
				return;

			installers.Remove(pInstaller);
			pInstaller.Completed -= OnInstallationCompleted;

			if (GetAssetName(true) == pInstaller.AssetName)
			{
				if (pResult == Installer.Result.Installed)
				{
					installButton.Disabled = true;
					installButton.Text = "Installed";
				}
				else
				{
					installButton.Disabled = false;
					installButton.Text = "Install";
				}
			}
		}

		protected bool IsInstalled()
		{
			return Directory.Exists(Config.InstallDir + "/" + GetAssetName(false));
		}

		protected void SetInstallButton()
		{
			if (installers.Count > 0)
			{
				string lAssetName = GetAssetName(true);

				for (int i = 0; i < installers.Count; i++)
				{
					if (installers[i].AssetName == lAssetName)
					{
						SetInstalling();
						return;
					}
				}
			}

			installButton.Disabled = IsInstalled();
			installButton.Text = installButton.Disabled ? "Installed" : "Install";
		}

		protected void SetInstalling()
		{
			installButton.Disabled = true;
			installButton.Text = "Installing...";
		}

		#endregion //INSTALLATION

		protected void AddItem(OptionButton pButton, Enum pItem)
		{
			pButton.AddItem(pItem.ToString(), Convert.ToInt32(pItem));
		}

		protected Source GetSource(ReleaseAsset pAsset)
		{
			OS lOS = Source.GetOS(pAsset);

			return new Source(
				pAsset,
				Version,
				Source.IsMono(pAsset),
				lOS,
				lOS == OS.MacOS ? null : Source.GetArchitecture(pAsset)
			);
		}

		protected string GetAssetName(bool pIsFile)
		{
			if (!nameIsValid)
			{
				assetName = assetNamePrefix;

				if (monoCheck.ButtonPressed)
				{
					assetName += "_mono";
				}

				assetName += GetOS() + FILE_TYPE;
				nameIsValid = true;
			}

			return pIsFile ? assetName : assetName[0..^4];
		}

		protected string GetOS()
		{
			if (osButton.Selected == (int)OS.MacOS)
			{
				return "_macos.universal";
			}

			string lOS;

			if (osButton.Selected == (int)OS.Linux)
			{
				lOS = $"_linux";
				lOS += monoCheck.ButtonPressed ? "_x86" : ".x86";
				lOS += architectureButton.Selected == (int)Architecture.x32 ? "_32" : "_64";
			}
			else
			{
				lOS = $"_win";
				lOS += architectureButton.Selected == (int)Architecture.x32 ? "32" : "64";

				if (!monoCheck.ButtonPressed)
				{
					lOS += ".exe";
				}
			}

			return lOS;
		}
	}
}
