using Godot;
using GlobalGameJam.Scripts.Core;

public partial class Player : CharacterBody2D
{
	private const uint BaseCollisionLayer = 0b1110;
	private const float ResetHeight = 1200f;

	[Export] public float MovementSpeed = 100f;

	[Export] public float JumpSpeed = -500f;

	public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private AnimationPlayer AnimationPlayer => GetNode<AnimationPlayer>("AnimationPlayer");
	
	private Sprite2D Head => GetNode<Sprite2D>("Sprites/Kopf");

	[Signal]
	public delegate void PlayerNeedsToBeResetEventHandler();

	public void SetActiveCollisionLayer(uint layer)
	{
		this.CollisionMask = BaseCollisionLayer | layer;
	}

	public override void _Ready()
	{
		this.Rotation = 0;
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
		if (direction > 0)
		{
			Head.FlipH = true;
		}

		if (direction < 0)
		{
			Head.FlipH = false;
		}

		if (Mathf.Abs(direction) > 0)
		{
			this.AnimationPlayer.Play("walking");
		}
		else
		{
			this.Rotation = 0;
			this.AnimationPlayer.Play("idle");
		}

		velocity.X = direction * this.MovementSpeed;
		this.Velocity = velocity;

		this.MoveAndSlide();

		if (this.GlobalPosition.Y > ResetHeight){
			this.EmitSignalPlayerNeedsToBeReset();
		}
	}
}
