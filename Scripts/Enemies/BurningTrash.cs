using Godot;

namespace GlobalGameJam.Scripts.Enemies;

public partial class BurningTrash : Trap
{
    public override void _Ready()
    {
        base._Ready();
        Sprite2D fireSprite = this.GetNode<Sprite2D>("Fire");
        ShaderMaterial material = (ShaderMaterial)fireSprite.Material.DuplicateDeep();
        NoiseTexture2D noiseTexture2D = (NoiseTexture2D)material.GetShaderParameter("noise_tex").As<NoiseTexture2D>().DuplicateDeep();
        ((FastNoiseLite)noiseTexture2D.Noise).Seed = GD.RandRange(0, int.MaxValue);
        material.SetShaderParameter("noise_tex", noiseTexture2D);
        fireSprite.Material = material;
    }
}