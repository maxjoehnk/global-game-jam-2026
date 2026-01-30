using Godot;
using GlobalGameJam.Scripts.Core;

public partial class PauseMenu : CenterContainer
{
    public void OnResume()
    {
        this.Visible = false;
        GetTree().Paused = false;
    }

    public void OnRestart()
    {
        SceneManager.Instance.RestartLevel();
    }

    public void OnExit()
    {
        SceneManager.Instance.OpenMainMenu();
    }
}
