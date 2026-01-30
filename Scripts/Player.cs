using Godot;
using GlobalGameJam.Scripts.Core;

public partial class Player : CharacterBody2D
{
	private const uint BaseCollisionLayer = 0b1110;
	
	[Export]
	public float MovementSpeed = 100f;
	
	[Export]
	public float JumpSpeed = -500f;
	
	public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public void SetActiveCollisionLayer(uint layer)
	{
		this.CollisionMask = BaseCollisionLayer | layer;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = this.Velocity;
		velocity.Y += this.Gravity * (float)delta;
		if (Input.IsActionJustPressed(InputAction.Jump) && this.IsOnFloor())
		{
			velocity.Y = this.JumpSpeed;
		}

		float direction = Input.GetAxis(InputAction.MoveLeft, InputAction.MoveRight);
		velocity.X = direction * this.MovementSpeed;
		this.Velocity = velocity;

		this.MoveAndSlide();
	}
}
