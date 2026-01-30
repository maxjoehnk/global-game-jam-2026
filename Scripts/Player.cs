using Godot;
using GlobalGameJam.Scripts.Core;

public partial class Player : Node2D
{
    [Export]
    public float MovementSpeed = 100f;
    
    [Export]
    public float JumpSpeed = -500f;
    
    public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    
    private CharacterBody2D Character => this.GetNode<CharacterBody2D>("CharacterBody2D");
    
    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = this.Character.Velocity;
        velocity.Y += this.Gravity * (float)delta;
        if (Input.IsActionJustPressed(InputAction.Jump) && this.Character.IsOnFloor())
        {
            velocity.Y = this.JumpSpeed;
        }

        float direction = Input.GetAxis(InputAction.MoveLeft, InputAction.MoveRight);
        velocity.X = direction * this.MovementSpeed;
        this.Character.Velocity = velocity;

        this.Character.MoveAndSlide();
    }
}
