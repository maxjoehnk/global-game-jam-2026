using Godot;
using Godot.Collections;

namespace GlobalGameJam.Scripts.Core.UserData;

public partial class PlayState : Resource
{
	[Export] public string? LastPlayedLevelName { get; set; }
	
	[Export] public Dictionary HighScores { get; set; } = new();
}