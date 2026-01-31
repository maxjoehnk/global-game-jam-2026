using Godot;

public partial class Projectile : RigidBody2D
{
    public override void _Ready()
    {
        this.BodyEntered += body =>
        {
            if (body is Enemy enemy)
            {
                enemy.Hit(this);
                this.QueueFree();
            }
        };
    }
}
