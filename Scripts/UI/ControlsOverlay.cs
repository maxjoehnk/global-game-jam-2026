using Godot;
using System.Collections.Generic;
using System.Linq;

// This will show the wrong input device if a gamepad is connected but the user is using keyboard & mouse
// It will switch to the correct icons on the first input
public partial class ControlsOverlay : HBoxContainer
{
    private InputType inputType = InputType.Keyboard;

    private IEnumerable<TextureRect> Textures => this.FindChildren("*", type: nameof(TextureRect)).Where(c => c is TextureRect).Cast<TextureRect>();

    public override void _Ready()
    {
        if (Input.GetConnectedJoypads().Count > 0)
        {
            this.SetGamepadIconForDevice(0);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton or InputEventKey)
        {
            this.inputType = InputType.Keyboard;
        }else if (@event is InputEventJoypadButton)
        {
            this.SetGamepadIconForDevice(@event.Device);
        }
    }

    private void SetGamepadIconForDevice(int deviceId)
    {
        string controllerName = Input.GetJoyName(deviceId);
        if (controllerName.Contains("PS") || controllerName.Contains("DualShock") ||
            controllerName.Contains("PlayStation"))
        {
            this.inputType = InputType.PlayStation;
        }
        else
        {
            this.inputType = InputType.GenericController;
        }
    }

    public override void _Process(double delta)
    {
        int textureOffset = (int)this.inputType * 256;
        
        foreach (TextureRect texture in this.Textures)
        {
            AtlasTexture? atlasTexture = texture.Texture as AtlasTexture;
            if (atlasTexture == null)
            {
                continue;
            }
            atlasTexture.Region = new Rect2(textureOffset, 0, 256, 256);
        }
    }

    enum InputType
    {
        GenericController = 0,
        PlayStation = 1,
        Keyboard = 2,
    }
}
