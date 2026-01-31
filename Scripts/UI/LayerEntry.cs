using Godot;

namespace GlobalGameJam.Scripts.UI;

public partial class LayerEntry : Button
{
	public bool Active
	{
		set => this.SetPressed(value);
	}

	public string LayerName
	{
		set => this.Text = value;
	}
}
