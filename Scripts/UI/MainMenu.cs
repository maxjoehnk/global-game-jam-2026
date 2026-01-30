using GlobalGameJam.Scripts.Core;
using Godot;

namespace GlobalGameJam.Scripts.UI;

public partial class MainMenu : Panel
{
    public void OnOpenSettings()
    {
        SceneManager.Instance.OpenSettings();
    }

    public void OnExit()
    {
        GetTree().Quit();
    }
}