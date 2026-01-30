using Godot;

namespace GlobalGameJam.Scripts.UI;

public partial class GameHud : Control
{
    private Label LayerIndicator => GetNode<Label>("LayerIndicator");
    
    public void SetActiveLayer(int layerIndex)
    {
        this.LayerIndicator.Text = $"Layer {layerIndex + 1}";
    }
}