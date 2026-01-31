using GlobalGameJam.Scripts.Core;
using Godot;

namespace GlobalGameJam.Scripts.UI;

public partial class MainMenu : Panel
{
	private Control LevelSelector => (this.FindChild("LevelSelector") as Control)!;

	public override void _Ready()
	{
		this.LevelSelector.GrabFocus();
	}

	public void OnOpenLevelSelector()
	{
		SceneManager.Instance.OpenLevelSelector();
	}
	
	public void OnOpenSettings()
	{
		SceneManager.Instance.OpenSettings();
	}

	public void OnExit()
	{
		GetTree().Quit();
	}
}
