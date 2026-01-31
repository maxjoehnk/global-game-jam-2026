using Godot;
using GlobalGameJam.Scripts.Core;

public partial class PauseMenu : CenterContainer
{
    private Control ResumeButton => (this.FindChild("Resume") as Control)!;

    public void ToggleDialog()
    {
        this.Visible = !this.Visible;
        if (this.Visible)
        {
            this.ResumeButton.GrabFocus();
        }
    }

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