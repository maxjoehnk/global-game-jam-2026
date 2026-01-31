using Godot;

public partial class SpahnKopf : TextureRect
{
    public void Animate()
    {
        this.RotationDegrees = GD.RandRange(-5, 5);
        if (GD.RandRange(0, 10) > 8)
        {
            this.FlipH = !this.FlipH;
        }
    }
}
