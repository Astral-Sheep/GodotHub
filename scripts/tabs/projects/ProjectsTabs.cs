using Com.Astral.GodotHub.Settings;
using Godot;

namespace Com.Astral.GodotHub.Tabs.Projects
{
	public partial class ProjectsTabs : Tab
	{
		[Export] protected PackedScene projectItemScene;
		[Export] protected Control itemContainer;

		public override void _Ready()
		{
			for (int i = 0; i < Files.Projects.Count; i++)
			{
				CreateItem(Files.Projects[i]);
			}
		}

		protected void CreateItem(string pPath)
		{
			ProjectItem lItem = projectItemScene.Instantiate<ProjectItem>();
			itemContainer.AddChild(lItem);
			lItem.Init(pPath);
		}
	}
}
