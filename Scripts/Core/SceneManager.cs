using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GlobalGameJam.Scripts.Core;

public partial class SceneManager : Node
{
    public static SceneManager Instance = null!;
    private Node CurrentScene { get; set; } = null!;
    
    public List<AvailableLevel> Levels { get; private set; }
    
    public override void _Ready()
    {
        Instance = this;
        Viewport root = this.GetTree().Root;
        this.CurrentScene = root.GetChild(-1);
        this.Levels = GetAvailableLevels();
    }

    public void OpenMainMenu()
    {
        this.LoadScene("res://Scenes/UI/MainMenu.tscn");
    }

    public void OpenLevelSelector()
    {
        this.LoadScene("res://Scenes/UI/LevelSelector.tscn");
    }

    public void OpenSettings()
    {
		this.LoadScene("res://Scenes/UI/Settings.tscn");
    }

    public void OpenLevel(AvailableLevel level)
    {
        this.LoadScene($"res://Scenes/Levels/{level.Path}");
    }

    private static List<AvailableLevel> GetAvailableLevels()
    {
        List<AvailableLevel> levels = ResourceLoader.ListDirectory("res://Scenes/Levels")
            .Where(name => name.EndsWith(".tscn"))
            .Where(name => !name.StartsWith("_"))
            .Select((file) => new AvailableLevel(file))
            .ToList();

        return levels;
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