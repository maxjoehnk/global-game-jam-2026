using GlobalGameJam.Scripts.Core;
using Godot;

namespace GlobalGameJam.Scripts.UI;

public partial class MainMenu : Panel
{
	private Control PlayButton => (this.FindChild("Play") as Control)!;
	private Button LevelSelector => (this.FindChild("LevelSelector") as Button)!;

	public override void _Ready()
	{
		this.PlayButton.GrabFocus();
		this.LevelSelector.Disabled = !SceneManager.Instance.HasUnlockedALevel;
		GlobalAudioPlayback.Instance.StartAllPlayback();
	}

	public void OnPlay()
	{
		SceneManager.Instance.LoadCurrentLevel();
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
