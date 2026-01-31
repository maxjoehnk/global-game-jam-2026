using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GlobalGameJam.Scripts.Core;

public partial class SceneManager : Node
{
	public static SceneManager Instance { get; private set; } = null!;
	private Node CurrentScene { get; set; } = null!;

	private AvailableLevel? activeLevel;
	
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
		GlobalAudioPlayback.Instance.StartAllPlayback();
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
		GlobalAudioPlayback.Instance.StopAllPlayback();
		this.activeLevel = level;
	}

	public void RestartLevel()
	{
		if (this.activeLevel == null)
		{
			return;
		}
		this.OpenLevel(this.activeLevel);
	}

	public void LoadNextLevel()
	{
		if (this.activeLevel == null)
		{
			return;
		}

		int nextLevelIndex = this.Levels.IndexOf(this.activeLevel) + 1;
		if (nextLevelIndex >= this.Levels.Count)
		{
			return;
		}
		
		this.OpenLevel(this.Levels[nextLevelIndex]);
	}

	private static List<AvailableLevel> GetAvailableLevels()
	{
		List<AvailableLevel> levels = ResourceLoader.ListDirectory("res://Scenes/Levels")
			.Where(name => name.EndsWith(".tscn"))
			.Where(name => !name.StartsWith("_"))
			.Select((file, index) => new AvailableLevel(file, index))
			.Where(level => !level.IsTestLevel || OS.IsDebugBuild())
			.OrderBy(level => level.LevelIndex)
			.ToList();

		return levels;
	}

	private void LoadScene(string path)
	{
		this.CurrentScene.QueueFree();
		this.activeLevel = null;
		PackedScene scene = GD.Load<PackedScene>(path);
		this.CurrentScene = scene.Instantiate();

		this.GetTree().Root.AddChild(this.CurrentScene);
		this.GetTree().CurrentScene = this.CurrentScene;
		this.GetTree().Paused = false;
	}
}
