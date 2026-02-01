using Godot;
using GlobalGameJam.Scripts;

public partial class CustomTabBar : CanvasLayer
{
	private PackedScene TabLabel => GD.Load<PackedScene>("res://Scenes/UI/tab_label.tscn");
	private Color FadeColor = new Color(0.4f, 0.4f, 0.4f);
	private HBoxContainer LayerContainer => GetNode<HBoxContainer>("OuterVBox/LabelBox");
	private TextureRect TextureBorder => GetNode<TextureRect>("OuterVBox/TextureRect");
	private Label TimeLabel => GetNode<Label>("TimerBox/LabelBox/TimeLabel");
	public void AddLabel(string Name, Color col)
	{
		Label NewLabel = (Label)this.TabLabel.Instantiate();
		this.LayerContainer.AddChild(NewLabel);
		NewLabel.Text = Name;
		NewLabel.Modulate = col;
	}
	public void SetActiveLayer(int layerIndex)
	{
		foreach (Label child in this.LayerContainer.GetChildren())
		{
			child.SelfModulate = this.FadeColor;
		}
		Label ActiveLabel = (Label)this.LayerContainer.GetChild(layerIndex);
		ActiveLabel.SelfModulate = new Color(1.0f, 1.0f, 1.0f);
		this.TextureBorder.Modulate = ActiveLabel.Modulate;
	}
	public void SetTimeLabel(double totalSeconds)
	{
		this.TimeLabel.Text = $"Time : {TimeFormat.Format(totalSeconds)}";
	}
}
