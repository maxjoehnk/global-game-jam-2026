using Godot;
using GlobalGameJam.Scripts.Core;

public partial class WonMenu : CenterContainer
{
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