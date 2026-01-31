using System.Linq;
using GlobalGameJam.Scripts.Core;
using Godot;
using Godot.Collections;

namespace GlobalGameJam.Scripts.UI;

public partial class GameHud : Control
{
    private PackedScene LayerEntryScene => GD.Load<PackedScene>("res://Scenes/UI/LayerEntry.tscn");

    private Control LayerList => GetNode<Control>("Panel/VBoxContainer/Layers");

    private PauseMenu PauseDialog => GetNode<PauseMenu>("PauseDialog");

    private WonMenu WonDialog => GetNode<WonMenu>("WonDialog");

    public override void _Ready()
    {
        this.ProcessMode = ProcessModeEnum.Always;
        this.PauseDialog.ProcessMode = ProcessModeEnum.WhenPaused;
        this.WonDialog.ProcessMode = ProcessModeEnum.WhenPaused;
        this.WonDialog.Visible = false;
        this.PauseDialog.Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
		if (Input.IsActionJustPressedByEvent(InputAction.Menu, @event))
		{
			this.PauseDialog.ToggleDialog();
		}
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

    public void ShowWonMenu()
    {
        this.WonDialog.ShowDialog();
    }
}