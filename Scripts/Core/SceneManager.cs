using System.Collections.Generic;
using System.Linq;
using GlobalGameJam.Scripts.Core.UserData;
using Godot;

namespace GlobalGameJam.Scripts.Core;

public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; } = null!;
    private Node CurrentScene { get; set; } = null!;

    public AvailableLevel? ActiveLevel { get; private set; }

    private int? lastFinishedLevelIndex;
    private int? currentLevelIndex;

    public List<AvailableLevel> Levels { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        Viewport root = this.GetTree().Root;
        this.CurrentScene = root.GetChild(-1);
        this.Levels = GetAvailableLevels();
        PlayState? playState = UserDataManager.LoadUserData();
        if (playState != null)
        {
            this.lastFinishedLevelIndex = this.Levels.FindIndex(level => level.Name == playState.LastPlayedLevelName);
            this.currentLevelIndex = this.lastFinishedLevelIndex + 1;
            foreach (KeyValuePair<Variant, Variant> highScore in playState.HighScores)
            {
                string levelName = highScore.Key.As<string>();
                double time = highScore.Value.As<double>();
                AvailableLevel? level = this.Levels.Find(level => level.Name == levelName);
                if (level != null)
                {
                    level.HighScore = time;
                }
            }
        }

        this.UnlockLevels();
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
        this.ActiveLevel = level;
    }

    public void RestartLevel()
    {
        if (this.ActiveLevel == null)
        {
            return;
        }

        this.OpenLevel(this.ActiveLevel);
    }

    public void LoadCurrentLevel()
    {
        AvailableLevel currentLevel =
            this.currentLevelIndex == null ? this.Levels.First() : this.Levels[this.currentLevelIndex.Value];
        this.OpenLevel(currentLevel);
    }

    public void LoadNextLevel()
    {
        if (this.ActiveLevel == null)
        {
            return;
        }

        int nextLevelIndex = this.Levels.IndexOf(this.ActiveLevel) + 1;
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
        this.ActiveLevel = null;
        PackedScene scene = GD.Load<PackedScene>(path);
        this.CurrentScene = scene.Instantiate();

        this.GetTree().Root.AddChild(this.CurrentScene);
        this.GetTree().CurrentScene = this.CurrentScene;
        this.GetTree().Paused = false;
    }

    private void UnlockLevels()
    {
        if (OS.IsDebugBuild())
        {
            foreach (AvailableLevel level in this.Levels)
            {
                level.IsUnlocked = true;
            }

            return;
        }

        for (int i = 0; i < this.Levels.Count; i++)
        {
            if (i == 0)
            {
                this.Levels[i].IsUnlocked = true;
                continue;
            }

            this.Levels[i].IsUnlocked = this.lastFinishedLevelIndex != null && i <= this.lastFinishedLevelIndex + 1;
        }
    }

    public void FinishedLevel(double levelTime)
    {
        if (this.ActiveLevel == null)
        {
            return;
        }
        this.ActiveLevel.HighScore = levelTime;
        this.lastFinishedLevelIndex = this.Levels.IndexOf(this.ActiveLevel);
		UserDataManager.StoreFinishedLevel(this.ActiveLevel!, levelTime);
        this.UnlockLevels();
    }
}