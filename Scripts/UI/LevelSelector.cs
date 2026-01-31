using System.Linq;
using GlobalGameJam.Scripts.Core;
using Godot;

namespace GlobalGameJam.Scripts.UI;

public partial class LevelSelector : Control
{
	private PackedScene LevelButton => GD.Load<PackedScene>("res://Scenes/UI/LevelButton.tscn");
	
	private Control? FirstLevel => this.GetChildren().Where(c => c is LevelButton).Cast<LevelButton>().FirstOrDefault();
	
	public override void _Ready()
	{
		foreach (AvailableLevel level in SceneManager.Instance.Levels)
		{
			LevelButton levelButton = this.CreateLevelButton(level);

			this.AddChild(levelButton);
		}
		
		FirstLevel?.GrabFocus();
	}

	private LevelButton CreateLevelButton(AvailableLevel level)
	{
		LevelButton levelButton = this.LevelButton.Instantiate<LevelButton>();
		levelButton.LevelName = level.Name;
		levelButton.Pressed += () => { SceneManager.Instance.OpenLevel(level); };
		
		return levelButton;
	}

	public void OnBackPressed()
	{
		SceneManager.Instance.OpenMainMenu();
	}
}
