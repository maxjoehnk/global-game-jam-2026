using Godot;

namespace GlobalGameJam.Scripts.Core.UserData;

public partial class Settings : Resource
{
    [Export]
    public double MainVolume { get; set; }
    
    [Export]
    public double MusicVolume { get; set; }
    
    [Export]
    public double EffectsVolume { get; set; }
}