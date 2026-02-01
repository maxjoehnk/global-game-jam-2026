using GlobalGameJam.Scripts;
using Godot;
using GlobalGameJam.Scripts.Core;

public partial class WonMenu : CenterContainer
{
	private Control NextLevelButton => (this.FindChild("NextLevel") as Control)!;
	private Label ScoreLabel => (this.FindChild("Score") as Label)!;
	
	public void ShowDialog(double time)
	{
		this.Visible = true;
		this.ScoreLabel.Text = TimeFormat.Format(time);
		this.GetTree().Paused = true;
		this.NextLevelButton.GrabFocus();
	}
	
	public void OnExit()
	{
		SceneManager.Instance.OpenMainMenu();
	}

	public void Restart()
	{
		SceneManager.Instance.RestartLevel();
	}

	public void NextLevel()
	{
		SceneManager.Instance.LoadNextLevel();
	}
}
