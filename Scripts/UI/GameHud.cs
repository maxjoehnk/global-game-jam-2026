using System.Linq;
using Godot;
using Godot.Collections;

namespace GlobalGameJam.Scripts.UI;

public partial class GameHud : Control
{
    private PackedScene LayerEntryScene => GD.Load<PackedScene>("res://Scenes/UI/LayerEntry.tscn");
    
    private Control LayerList => GetNode<Control>("Panel/VBoxContainer/Layers");
    
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
}