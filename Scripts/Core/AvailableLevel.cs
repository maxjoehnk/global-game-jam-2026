namespace GlobalGameJam.Scripts.Core;

public class AvailableLevel
{
	public string Path { get; }

	public string Name { get; }

	public AvailableLevel(string path)
	{
		this.Path = path;
		this.Name = path.Replace(".tscn", "");
	}
}