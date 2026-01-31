using System.Linq;
using Godot;
using Godot.Collections;

namespace GlobalGameJam.Scripts.UI;

public partial class GameHud : Control
{
    private PackedScene LayerEntryScene => GD.Load<PackedScene>("res://Scenes/UI/LayerEntry.tscn");
    
    private Control LayerList => GetNode<Control>("Panel/VBoxContainer/Layers");
    
    private Control PauseDialog => GetNode<Control>("PauseDialog");
    
    private Control WonDialog => GetNode<Control>("WonDialog");

    public override void _Ready()
    {
        this.PauseDialog.ProcessMode = ProcessModeEnum.WhenPaused;
        this.WonDialog.ProcessMode = ProcessModeEnum.WhenPaused;
        this.WonDialog.Visible = false;
        this.PauseDialog.Visible = false;
    }

    public void SetActiveLayer(int layerIndex)
    {
        foreach (LayerEntry child in this.LayerList.GetChildren().Cast<LayerEntry>())
        {
            child.Active = false;
        }

        this.LayerList.GetChildren().Cast<LayerEntry>().ElementAt(layerIndex).Active = true;
    }

    public void SetLayers(Array<string> layers)
    {
        foreach (string layerName in layers)
        {
            LayerEntry layer = this.LayerEntryScene.Instantiate<LayerEntry>();
            layer.LayerName = layerName;
            this.LayerList.AddChild(layer);
        }
    }

    public void TogglePauseMenu()
    {
        this.PauseDialog.Visible = !this.PauseDialog.Visible;
    }

    public void ShowWonMenu()
    {
        this.WonDialog.Visible = true;
    }
}