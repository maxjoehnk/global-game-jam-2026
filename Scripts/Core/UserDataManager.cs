using GlobalGameJam.Scripts.Core.UserData;
using Godot;

namespace GlobalGameJam.Scripts.Core;

public partial class UserDataManager : GodotObject
{
	public static void StoreFinishedLevel(AvailableLevel level, double playTime)
	{
		PlayState playState = LoadUserData() ?? new PlayState();
		playState.LastPlayedLevelName = level.Name;
		playState.HighScores[level.Name] = playTime;
		ResourceSaver.Save(playState, "user://user_data.tres");
	}
	
	public static PlayState? LoadUserData()
	{
		if (!FileAccess.FileExists("user://user_data.tres"))
		{
			return null;
		}
		PlayState? playState = ResourceLoader.Load<PlayState>("user://user_data.tres");

		return playState;
	}

	public static void ClearUserData()
	{
		ResourceSaver.Save(new PlayState(), "user://user_data.tres");
	}
}