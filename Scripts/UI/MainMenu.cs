using GlobalGameJam.Scripts.Core;
using Godot;

namespace GlobalGameJam.Scripts.UI;

public partial class MainMenu : Panel
{
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
