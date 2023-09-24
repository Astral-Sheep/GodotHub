using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Debug;
using Com.Astral.GodotHub.Utils.Comparisons;
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
	public partial class ReleaseItem : Control, IVersionItem, IValidItem
	{
		protected const string RELEASE_NAME_SUFFIX = "-stable";
		protected const string ASSET_NAME_PREFIX = "Godot_v";
		protected const string FILE_TYPE = ".zip";

		/// <summary>
		/// Event called when the install button is pressed,
		/// thus requiring an installation of the given <see cref="Source"/>
		/// </summary>
		public static event Action<ReleaseItem, Source> InstallPressed;

		/// <summary>
		/// Index of the release. 0 corresponds to the latest
		/// </summary>
		public int Index { get; protected set; }
		public Version Version { get; protected set; }
		public bool IsValid => true;

		[Export] protected Label versionLabel;
		[Export] protected Label dateLabel;
		[Export] protected CheckBox monoCheck;
		[Export] protected OptionButton osButton;
		[Export] protected OptionButton architectureButton;
		[Export] protected Button installButton;

		protected List<Source> sources;
		protected List<Installer> installers;
		protected string assetNamePrefix;

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

			osButton.Selected = (int)AppConfig.os;
			architectureButton.Selected = (int)AppConfig.architecture;
		}

		protected override void Dispose(bool pDisposing)
		{
			if (!pDisposing)
				return;

			InstallItem.Closed -= OnInstallItemClosed;
		}

		/// <summary>
		/// Set data to correspond to the <see cref="Release"/>
		/// </summary>
		/// <param name="pRelease"></param>
		/// <param name="pIndex"><see cref="Release"/> index in the list of <see cref="Release"/>s</param>
		public void Init(Release pRelease, int pIndex)
		{
			Index = pIndex;
			assetNamePrefix = $"{ASSET_NAME_PREFIX}{pRelease.Name}";
			Version lVersion = (Version)pRelease.Name;
			Version = Source.GetVersion(pRelease);

			installers = new List<Installer>();
			sources = new List<Source>();
			ReleaseAsset lReleaseAsset;

			for (int i = 0; i < pRelease.Assets.Count; i++)
			{
				lReleaseAsset = pRelease.Assets[i];

				if (lReleaseAsset.Name[0] != ASSET_NAME_PREFIX[0] || lReleaseAsset.Name[^4..] != FILE_TYPE)
					continue;

				sources.Add(GetSource(lReleaseAsset));
			}

			versionLabel.Text = $"Godot {lVersion}";
			dateLabel.Text = $"{pRelease.CreatedAt.Day:D2}/{pRelease.CreatedAt.Month:D2}/{pRelease.CreatedAt.Year:D4}";
			SetInstallButton();

			InstallItem.Closed += OnInstallItemClosed;
		}

		#region EVENT_HANDLING

		protected void OnMonoChanged(bool _)
		{
			SetInstallButton();
		}

		protected void OnOsChanged(long pOS)
		{
			architectureButton.Visible = pOS != (long)OS.MacOS;
			SetInstallButton();
		}

		protected void OnArchitectureChanged(long _)
		{
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
					InstallPressed?.Invoke(this, source);
					return;
				}
			}

			//To do: create error popup
			Debugger.PrintError($"No asset named {lAssetName}");
		}

		protected void OnInstallItemClosed(InstallItem pItem)
		{
			if (pItem.Version == Version)
			{
				SetInstallButton();
			}
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

		protected void OnInstallationCompleted(Installer pInstaller, InstallT.Result pResult)
		{
			if (!installers.Contains(pInstaller))
				return;

			installers.Remove(pInstaller);
			pInstaller.Completed -= OnInstallationCompleted;

			if (GetAssetName(true) == pInstaller.AssetName)
			{
				if (pResult == InstallT.Result.Installed)
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
			return Directory.Exists(AppConfig.InstallDir + "/" + GetAssetName(false));
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
			string lAssetName = assetNamePrefix;

			if (monoCheck.ButtonPressed)
			{
				lAssetName += "_mono";
			}

			lAssetName += GetOS(pIsFile) + FILE_TYPE;
			return pIsFile ? lAssetName : lAssetName[..^4];
		}

		protected string GetOS(bool pIsFile)
		{
			if (osButton.Selected == (int)OS.MacOS)
			{
				return "_macos.universal";
			}

			string lOS;

			if (osButton.Selected == (int)OS.Linux)
			{
				lOS = $"_linux";
				lOS += pIsFile && monoCheck.ButtonPressed ? "_x86" : ".x86";
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
