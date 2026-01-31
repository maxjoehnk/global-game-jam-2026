using Godot;
using GlobalGameJam.Scripts.Core;

public enum State
{
	Idle,
	Run,
	Jump,
	Fall, 
	WallJump,
	Dive
}


public partial class Player : CharacterBody2D
{
	private const uint BaseCollisionLayer = 0b1110;
	private const float ResetHeight = 1200f;
	private const float MoveTol = 0.01f;
	// Physics
	[Export] public float MovementSpeed = 260f;
	[Export] public float MovementAccel = 400f;
	[Export] public float MovementFriction = 300f;
	[Export] public float MovementStartDash = 70f;

	[Export] public float JumpSpeed = -650f;
	[Export] public float AirSpeed = 250f;
	[Export] public float AirAccel = 450f;

	[Export] public float WallJumpX = 400f;
	[Export] public float WallJumpY = -600f;
	[Export] public float WallJumpInputLimit = -0.9f;
	private Timer WallJumpTimer => GetNode<Timer>("WallJumpTimer");
	private float LastWallDir = 0.0f;

	[Export] public float FallPull = 400f;
	[Export] public float FallTransition = 70f;
	private Timer CoyoteTimer => GetNode<Timer>("CoyoteTimer");

	[Export] public float DiveStrength = 350f;

	public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	// Graphics
	private AnimationPlayer AnimationPlayer => GetNode<AnimationPlayer>("AnimationPlayer");
	private State CurrentState = State.Idle;
	private Sprite2D Head => GetNode<Sprite2D>("Sprites/Kopf");

	// Interact with world:
	private const float CutPosX = 75.0f;
	private TileMapCutter CutTool => GetNode<TileMapCutter>("TileMapCutter");

	[Signal]
	public delegate void PlayerDiedEventHandler();

	public void SetActiveCollisionLayer(uint layer)
	{
		this.CutTool.SetActiveLayers(layer);
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

			case State.WallJump:
				this.jump_state(delta);
				break;

			case State.Dive:
				this.dive_state(delta);
				break;
		}

		this.MoveAndSlide();

		if (this.GlobalPosition.Y > ResetHeight){
			this.EmitSignalPlayerDied();
		}
		// Put cut area in front of player
		this.CutTool.Position = new Vector2(
				Mathf.Sign(this.Velocity.X) * CutPosX, 0.0f
			);
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
				this.AnimationPlayer.Play("idle");
				velocity.Y = this.JumpSpeed;
				this.Velocity = velocity;
				break;
			
			case State.Fall:
				if (this.CurrentState != State.Jump && this.CurrentState != State.WallJump){
					this.CoyoteTimer.Start();
				}
				velocity.Y += this.FallPull;
				this.Velocity = velocity;
				break;
			case State.WallJump:
				this.AnimationPlayer.Play("idle");
				velocity.Y = this.WallJumpY;
				velocity.X = this.LastWallDir * this.WallJumpX;
				this.Velocity = velocity;
				break;
			case State.Dive:
				float direction = Input.GetAxis(InputAction.MoveLeft, 
												InputAction.MoveRight);
				if (direction == 0)
				{
					direction = Mathf.Sign(this.Velocity.X);
				}
				velocity.X += this.DiveStrength * direction;
				this.Velocity = velocity;
				if (direction < 0)
				{
					this.AnimationPlayer.Play("dive_left");
				}
				else
				{
					this.AnimationPlayer.Play("dive_right");
				}
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
			Vector2 velocity = this.Velocity;
			velocity.X = this.MovementStartDash * direction;
			this.Velocity = velocity;
			this.set_new_state(State.Run);
		}
	}

	public void update_look_direction(float direction){
		Head.FlipH = direction > 0;
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
		if (Mathf.Abs(direction) > MoveTol && Mathf.Abs(velocity.X) < this.MovementSpeed)
		{
			velocity.X = Mathf.MoveToward(
				velocity.X, 
				direction * this.MovementSpeed, 
				(float)delta * this.MovementAccel * this.scale_accel(direction * velocity.X > 0)
			);	
		}
		if (Mathf.Abs(direction) < MoveTol){
			velocity.X = 0.0f;
		}
		else if (direction * velocity.X < 0.0f){
			velocity.X = direction * this.MovementStartDash;
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
			AccelScale *= 5.0f;
		}
		return AccelScale;
	}

	public void jump_state(double delta){
		if (this.Velocity.Y >= this.FallTransition){
			this.set_new_state(State.Fall);
			return;
		}
		else if (this.IsOnFloor())
		{
			this.set_new_state(State.Run);
			return;
		}
		else if (Input.IsActionJustPressed("dive"))
		{
			this.set_new_state(State.Dive);
			return;			
		}
		Vector2 velocity = this.Velocity;
		float direction = Input.GetAxis(InputAction.MoveLeft, InputAction.MoveRight);
		this.update_look_direction(direction);
		velocity.Y += this.Gravity * (float)delta;;
		if (Mathf.Abs(velocity.X) < this.AirSpeed || velocity.X * direction < 0.0f)
		{
			velocity.X = Mathf.MoveToward(
			velocity.X, 
			direction * this.AirSpeed, 
			(float)delta * this.AirAccel * this.scale_accel(direction * velocity.X > 0));	
		}
		this.Velocity = velocity;
		if (this.check_wall_jump(direction))
		{
			this.set_new_state(State.WallJump);
			return;
		}
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
		else if (Input.IsActionJustPressed("dive"))
		{
			this.set_new_state(State.Dive);
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
		if (this.check_wall_jump(direction))
		{
			this.set_new_state(State.WallJump);
			return;
		}
	}

	public bool check_wall_jump(float input_x){
		if (this.WallJumpTimer.TimeLeft > 0 && Input.IsActionJustPressed("jump"))
		{
			this.WallJumpTimer.Stop();
			return true;
		}
		if (this.IsOnWall())
		{
			this.LastWallDir = this.GetWallNormal().X;
			if(this.GetWallNormal().X * input_x < this.WallJumpInputLimit && Input.IsActionJustPressed("jump"))
			{
				return true;
			}
			else if (this.WallJumpTimer.TimeLeft == 0)
			{
				this.WallJumpTimer.Start();
			}
		}
		return false;
	}

	public void dive_state(double delta){
		Vector2 velocity = this.Velocity;
		if (this.IsOnFloor())
		{
			velocity.X /= 2.0f;
			this.Velocity = velocity;
			this.set_new_state(State.Run);
			return;
		}
		velocity.Y += this.Gravity * (float)delta;
		this.Velocity = velocity;
		float direction = Input.GetAxis(InputAction.MoveLeft, InputAction.MoveRight);
		if (this.check_wall_jump(direction))
		{
			this.set_new_state(State.WallJump);
		}
	}

}
