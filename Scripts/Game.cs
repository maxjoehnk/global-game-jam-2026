using Godot;
using GlobalGameJam.Scripts.Core;

public partial class Game : Node2D
{
    private Node2D LayerContainer => GetNode<Node2D>("Layers");

    private int LayerCount => LayerContainer.GetChildCount();
    
    private int activeLayerIndex;
    
    private Node2D ActiveLayer => LayerContainer.GetChild<Node2D>(this.activeLayerIndex);

    public override void _Ready()
    {
        this.UpdateActiveLayer();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(InputAction.LayerUp))
        {
            GD.Print("Next Layer");
            this.NextLayer();
        }

        if (Input.IsActionJustPressed(InputAction.LayerDown))
        {
            GD.Print("Previous Layer");
            this.PreviousLayer();
        }
    }

    private void NextLayer()
    {
        this.activeLayerIndex += 1;
        if (this.activeLayerIndex >= this.LayerCount)
        {
            this.activeLayerIndex = 0;
        }
        this.UpdateActiveLayer();
    }

    private void PreviousLayer()
    {
        this.activeLayerIndex -= 1;
        if (this.activeLayerIndex < 0)
        {
            this.activeLayerIndex = this.LayerCount - 1;
        }
        this.UpdateActiveLayer();
    }

    private void UpdateActiveLayer()
    {
        GD.Print($"Active Layer: {this.activeLayerIndex}");
        for (int i = 0; i < this.LayerContainer.GetChildren().Count; i++)
        {
            Node2D layer = this.LayerContainer.GetChild<Node2D>(i);
            layer.ProcessMode = i == this.activeLayerIndex ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
            layer.Visible = i == this.activeLayerIndex;
        }
    }
}
