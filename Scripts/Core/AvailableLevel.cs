using System.Text.RegularExpressions;

namespace GlobalGameJam.Scripts.Core;

public partial class AvailableLevel
{
	private const int FileIndexOffset = 1000;
	private const int TestIndexOffset = -1000;

	public string Path { get; }

	public string Name { get; }

	public int LevelIndex { get; }

	/// <summary>
	/// Level that is only available in Debug Builds.
	///
	/// Prefix Level Files with "test_" to mark them as test levels.
	/// </summary>
	public bool IsTestLevel { get; set; }

	public bool IsUnlocked { get; set; }

	public AvailableLevel(string path, int fileIndex)
	{
		this.Path = path;
		Match match = LevelNameRegex().Match(path);
		this.Name = match.Groups["Name"].Value;
		this.IsTestLevel = match.Groups["IsTest"].Success;
		if (match.Groups["Id"].Success)
		{
			this.LevelIndex = int.Parse(match.Groups["Id"].Value);
		}
		else if (this.IsTestLevel)
		{
			this.LevelIndex = TestIndexOffset + fileIndex;
		}
		else
		{
			this.LevelIndex = FileIndexOffset + fileIndex;
		}
	}

	[GeneratedRegex("(?<IsTest>test_)?((?<Id>[0-9]+)_)?(?<Name>.*).tscn",
		RegexOptions.Compiled | RegexOptions.IgnoreCase)]
	private static partial Regex LevelNameRegex();
}