using Com.Astral.GodotHub.Data;
using Godot;
using System.Collections.Generic;

namespace Com.Astral.GodotHub.Tabs.Projects
{
	public partial class ProjectsTabs : Tab
	{
		[Export] protected PackedScene projectItemScene;
		[Export] protected Control itemContainer;

		public override void _Ready()
		{
			List<GDFile> lProjects = ProjectsData.GetProjects();

			for (int i = 0; i < lProjects.Count; i++)
			{
				CreateItem(lProjects[i]);
			}
		}

		protected void CreateItem(GDFile pProject)
		{
			ProjectItem lItem = projectItemScene.Instantiate<ProjectItem>();
			itemContainer.AddChild(lItem);
			lItem.Init(pProject);
		}

		protected override void Connect() { }

		protected override void Disconnect() { }
	}
}
