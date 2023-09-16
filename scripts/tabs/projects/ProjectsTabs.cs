using Com.Astral.GodotHub.Data;
using Godot;
using System.Collections.Generic;

namespace Com.Astral.GodotHub.Tabs.Projects
{
	public partial class ProjectsTabs : Tab
	{
		[Export] protected PackedScene projectItemScene;
		[Export] protected Control itemContainer;
		[ExportGroup("Project addition")]
		[Export] protected PackedScene folderPopupScene;
		[Export] protected Button addButton;

		public override void _Ready()
		{
			List<GDFile> lProjects = ProjectsData.GetProjects();

			for (int i = 0; i < lProjects.Count; i++)
			{
				CreateItem(lProjects[i]);
			}
			
			addButton.Pressed += OnAddPressed;
		}

		protected void CreateItem(GDFile pProject)
		{
			ProjectItem lItem = projectItemScene.Instantiate<ProjectItem>();
			itemContainer.AddChild(lItem);
			lItem.Init(pProject);
		}

		protected override void Connect() { }

		protected override void Disconnect() { }

		protected void OnAddPressed()
		{
			FileDialog lDialog = folderPopupScene.Instantiate<FileDialog>();
			Main.Instance.AddChild(lDialog);
			lDialog.PopupCentered();
			lDialog.CurrentDir = Config.ProjectDir;
			lDialog.FileMode = FileDialog.FileModeEnum.OpenFiles;
			lDialog.Filters = new string[] { "*.godot" };
			lDialog.FileSelected += OnFileSelected;
			lDialog.FilesSelected += OnFilesSelected;
		}

		protected void OnFileSelected(string pPath)
		{
			pPath = pPath[..pPath.RFind("/project.godot")];
			GDFile lProject = new GDFile(pPath, false, ProjectsData.GetVersionFromFolder(pPath));
			ProjectsData.AddProject(lProject);
			CreateItem(lProject);
		}

		protected void OnFilesSelected(string[] pPaths)
		{
			for (int i = 0; i < pPaths.Length; i++)
			{
				OnFileSelected(pPaths[i]);
			}
		}
	}
}
