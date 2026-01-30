using Godot;

namespace GlobalGameJam.Scripts.Core;

public partial class SceneManager : Node
{
    public static SceneManager Instance = null!;
    private Node CurrentScene { get; set; } = null!;
    
    public override void _Ready()
    {
        Instance = this;
        Viewport root = this.GetTree().Root;
        this.CurrentScene = root.GetChild(-1);
    }

    public void OpenMainMenu()
    {
        this.LoadScene("res://Scenes/UI/MainMenu.tscn");
    }

    public void OpenSettings()
    {
		this.LoadScene("res://Scenes/UI/Settings.tscn");
    }

    private void LoadScene(string path)
    {
        this.CurrentScene.QueueFree();
        PackedScene scene = GD.Load<PackedScene>(path);
        this.CurrentScene = scene.Instantiate();

        this.GetTree().Root.AddChild(this.CurrentScene);
        this.GetTree().CurrentScene = this.CurrentScene;
    }
}