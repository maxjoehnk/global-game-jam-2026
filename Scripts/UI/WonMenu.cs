using Godot;
using GlobalGameJam.Scripts.Core;

public partial class WonMenu : CenterContainer
{
    private Control NextLevelButton => (this.FindChild("NextLevel") as Control)!;
    
    public void ShowDialog()
    {
        this.Visible = true;
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