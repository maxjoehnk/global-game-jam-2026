using System.Linq;
using GlobalGameJam.Scripts.Core;
using Godot;

namespace GlobalGameJam.Scripts.UI;

public partial class LevelSelector : Control
{
	private PackedScene LevelButton => GD.Load<PackedScene>("res://Scenes/UI/LevelSelectButton.tscn");
	
	private Control? FirstLevel => this.GetChildren().Where(c => c is Button).Cast<Button>().FirstOrDefault();
	
	public override void _Ready()
	{
		foreach (AvailableLevel level in SceneManager.Instance.Levels)
		{
			Button levelButton = this.CreateLevelButton(level);

			this.AddChild(levelButton);
		}
		
		FirstLevel?.GrabFocus();
	}

	private Button CreateLevelButton(AvailableLevel level)
	{
		LevelSelectButton button = this.LevelButton.Instantiate<LevelSelectButton>();
		button.Text = level.Name;
		button.Disabled = !level.IsUnlocked;
		button.IsTestLevel = level.IsTestLevel;
		button.HighScore = level.HighScore;
		button.Pressed += () => { SceneManager.Instance.OpenLevel(level); };
		
		return button;
	}

	public void OnBackPressed()
	{
		SceneManager.Instance.OpenMainMenu();
	}
}
