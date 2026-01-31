using Godot;
using GlobalGameJam.Scripts.Core;

public enum State
{
	Idle,
	Run,
	Jump,
	Fall
}


public partial class Player : CharacterBody2D
{
	private const uint BaseCollisionLayer = 0b1110;
	private const float ResetHeight = 1200f;
	private const float MoveTol = 0.01f;

	[Export] public float MovementSpeed = 140f;
	[Export] public float MovementAccel = 160f;
	[Export] public float MovementFriction = 85f;

	[Export] public float JumpSpeed = -700f;
	[Export] public float AirSpeed = 180f;
	[Export] public float AirAccel = 150f;

	[Export] public float FallPull = 100f;
	private Timer CoyoteTimer => GetNode<Timer>("CoyoteTimer");

	public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private AnimationPlayer AnimationPlayer => GetNode<AnimationPlayer>("AnimationPlayer");
	private State CurrentState = State.Idle;
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
		switch (this.CurrentState)
		{
			case State.Idle:
				this.idle_state(delta);
				break;

			case State.Run:
				this.run_state(delta);
				break;

			case State.Jump:
				this.jump_state(delta);
				break;
			
			case State.Fall:
				this.fall_state(delta);
				break;
		}

		this.MoveAndSlide();

		if (this.GlobalPosition.Y > ResetHeight){
			this.EmitSignalPlayerNeedsToBeReset();
		}
	}

	public void set_new_state(State NewState){
		Vector2 velocity = this.Velocity;
		switch (NewState)
		{
			case State.Idle:
				this.AnimationPlayer.Play("idle");
				break;

			case State.Run:
				this.AnimationPlayer.Play("walking");
				break;

			case State.Jump:
				velocity.Y = this.JumpSpeed;
				this.Velocity = velocity;
				break;
			
			case State.Fall:
				if (this.CurrentState != State.Jump){
					this.CoyoteTimer.Start();
				}
				velocity.Y += this.FallPull;
				this.Velocity = velocity;
				break;
		}
		this.CurrentState = NewState;
	}

	public void idle_state(double delta){
		if (Input.IsActionJustPressed(InputAction.Jump)){
			this.set_new_state(State.Jump);
		}
		else if (!this.IsOnFloor())
		{
			this.set_new_state(State.Fall);
		}
		float direction = Input.GetAxis(InputAction.MoveLeft, InputAction.MoveRight);
		if (Mathf.Abs(direction) > MoveTol){
			this.set_new_state(State.Run);
		}
	}

	public void update_look_direction(float direction){
		if (direction > 0)
		{
			Head.FlipH = true;
		}

		if (direction < 0)
		{
			Head.FlipH = false;
		}
	}

	public void run_state(double delta){
		if (Input.IsActionJustPressed(InputAction.Jump)){
			this.set_new_state(State.Jump);
			return;
		}
		else if (!this.IsOnFloor())
		{
			this.set_new_state(State.Fall);
			return;
		}
		Vector2 velocity = this.Velocity;
		float direction = Input.GetAxis(InputAction.MoveLeft, InputAction.MoveRight);
		this.update_look_direction(direction);
		velocity.Y = 0.0f;
		velocity.X = Mathf.MoveToward(
			velocity.X, 
			direction * this.MovementSpeed, 
			(float)delta * this.MovementAccel * this.scale_accel(direction * velocity.X > 0)
		);
		if (Mathf.Abs(direction) < MoveTol){
			velocity.X = Mathf.MoveToward(
				velocity.X, 0.0f, (float)delta * this.MovementFriction
			);
		}
		this.Velocity = velocity;
		if (Mathf.Abs(this.Velocity.X) < MoveTol)
		{
			this.set_new_state(State.Idle);
			return;
		}	
	}

	public float scale_accel(bool direction_in_velocity){
		float AccelScale = 1.0f;
		if (!direction_in_velocity){
			AccelScale *= 2.0f;
		}
		return AccelScale;
	}

	public void jump_state(double delta){
		if (this.Velocity.Y >= 0.0f){
			this.set_new_state(State.Fall);
			return;
		}
		else if (this.IsOnFloor())
		{
			this.set_new_state(State.Run);
			return;
		}
		Vector2 velocity = this.Velocity;
		float direction = Input.GetAxis(InputAction.MoveLeft, InputAction.MoveRight);
		this.update_look_direction(direction);
		velocity.Y += this.Gravity * (float)delta;;
		velocity.X = Mathf.MoveToward(
			velocity.X, 
			direction * this.AirSpeed, 
			(float)delta * this.AirAccel * this.scale_accel(direction * velocity.X > 0)
		);
		this.Velocity = velocity;
	}

	public void fall_state(double delta){
		if (this.CoyoteTimer.TimeLeft > 0.0f && Input.IsActionJustPressed(InputAction.Jump)){
			this.set_new_state(State.Jump);
			return;
		}
		else if (this.IsOnFloor())
		{
			this.set_new_state(State.Run);
			return;
		}
		Vector2 velocity = this.Velocity;
		float direction = Input.GetAxis(InputAction.MoveLeft, InputAction.MoveRight);
		this.update_look_direction(direction);
		velocity.Y += this.Gravity * (float)delta;;
		velocity.X = Mathf.MoveToward(
			velocity.X, direction * this.AirSpeed, (float)delta * this.AirAccel
		);
		this.Velocity = velocity;
	}

}
