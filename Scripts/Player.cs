using Godot;
using GlobalGameJam.Scripts.Core;

public enum State
{
	Idle,
	Run,
	Jump,
	Fall, 
	WallJump,
	Dive,
	Hurt
}


public partial class Player : CharacterBody2D
{
	private const uint BaseCollisionLayer = 0b1000;
	public float ResetHeight = 1200f;
	private const float MoveTol = 0.01f;
	private const float SpriteScale = 0.5f;
	// Physics
	[Export] public float MovementSpeed = 290f;
	[Export] public float MovementAccel = 450f;
	[Export] public float MovementFriction = 300f;
	[Export] public float MovementStartDash = 90f;

	[Export] public float JumpSpeed = -700f;
	[Export] public float AirSpeed = 270f;
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

	private bool DieAnimationDone = false;

	public Item? holdingItem;

	public bool HasAntrag = false;

	public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	// Graphics
	private AnimationPlayer AnimationPlayer => GetNode<AnimationPlayer>("AnimationPlayer");
	private AnimationTree AnimTree => GetNode<AnimationTree>("AnimationTree");
	private AnimationNodeStateMachinePlayback Playback => (AnimationNodeStateMachinePlayback)
			AnimTree.Get("parameters/StateMachine/playback");
	
	private State CurrentState = State.Idle;
	private Sprite2D Head => GetNode<Sprite2D>("Sprites/Kopf");
	private Node2D SpriteContainer => GetNode<Node2D>("Sprites");

	private Sprite2D MaskSprite => GetNode<Sprite2D>("Sprites/Kopf/Mask");
	private Sprite2D PaperSprite => GetNode<Sprite2D>("Sprites/ArmL/Antrag");


	// Interact with world:
	private const float CutPosX = 75.0f;
	private TileMapCutter CutTool => GetNode<TileMapCutter>("TileMapCutter");

	[Signal]
	public delegate void PlayerDiedEventHandler();

	[Signal]
	public delegate void PlayerStartedWalkingEventHandler();

	[Signal]
	public delegate void PlayerStoppedWalkingEventHandler();

	[Signal]
	public delegate void PlayerDivedEventHandler();

	[Signal]
	public delegate void PlayerJumpedEventHandler();

	[Signal]
	public delegate void PlayerLandedEventHandler();

	[Signal]
	public delegate void ItemChangedEventHandler(Item? item);

	public void SetActiveCollisionLayer(uint layer)
	{
		this.CutTool.SetActiveLayers(layer);
		this.CollisionMask = BaseCollisionLayer | layer;
	}

	public override void _Ready()
	{
		this.Rotation = 0;
		this.AnimTree.Active = true;
		this.MaskSprite.Visible = false;
		this.PaperSprite.Visible = false;
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressedByEvent(InputAction.Throw, @event))
		{
			if (this.holdingItem != null && this.CurrentState != State.Hurt)
			{
				this.holdingItem.Throw(this.GetParent(), this.GlobalPosition, this.Head.FlipH);
				this.PaperSprite.Visible = false;
				this.holdingItem = null;
				this.EmitSignalItemChanged(null);
				
				bool isThrowing = (bool)this.AnimTree.Get("parameters/OneShot/active");
				if (!isThrowing)
				{
					this.AnimTree.Set(
						"parameters/OneShot/request",
						(int)AnimationNodeOneShot.OneShotRequest.Fire
					);
				}
			}
		}
	}

	public bool HasMask()
	{
		return this.MaskSprite.Visible;
	}

	public void GotMask()
	{
		this.MaskSprite.Visible = true;
	}

	public void GotAntrag(bool got_it)
	{
		this.HasAntrag = got_it;
		this.PaperSprite.Visible = got_it;
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
				this.wall_jump_state(delta);
				break;

			case State.Dive:
				this.dive_state(delta);
				break;
			case State.Hurt:
				return;
		}

		this.MoveAndSlide();

		if (this.GlobalPosition.Y > ResetHeight){
			this.EmitSignalPlayerDied();
		}
		// Put cut area in front of player
		if (Mathf.Abs(this.Velocity.X) > 0)
		{
			this.CutTool.Position = new Vector2(
					Mathf.Sign(this.Velocity.X) * CutPosX, 0.0f
				);	
		}
	}

	public void set_new_state(State NewState){
		Vector2 velocity = this.Velocity;
		switch (NewState)
		{
			case State.Idle:
				this.Playback.Travel("idle");
				break;

			case State.Run:
				this.Playback.Travel("walking");
				this.EmitSignalPlayerStartedWalking();
				break;

			case State.Jump:
				this.Playback.Travel("jump");
				this.EmitSignalPlayerJumped();
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
				this.Playback.Travel("jump");
				this.EmitSignalPlayerJumped();
				velocity.Y = this.WallJumpY;
				velocity.X = this.LastWallDir * this.WallJumpX;
				this.Velocity = velocity;
				break;
			case State.Dive:
				this.EmitSignalPlayerDived();
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
					this.Playback.Travel("dive_left");
				}
				else
				{
					this.Playback.Travel("dive_right");
				}
				break;
			case State.Hurt:
				break;
		}

		if (this.CurrentState is State.Fall or State.Dive or State.Jump or State.WallJump &&
			NewState is State.Idle or State.Run)
		{
			this.EmitSignalPlayerLanded();
		}
		
		if (this.CurrentState == State.Run)
		{
			this.EmitSignalPlayerStoppedWalking();
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

	public void Reset()
	{
		this.Playback.Travel("RESET");
		this.CurrentState = State.Idle;
	}

	public void Hit(Vector2 HitSource)
	{
		if (!(this.CurrentState == State.Hurt))
		{
			this.DieAnimationDone = false;
			this.set_new_state(State.Hurt);
			if (HitSource.X > this.GlobalPosition.X)
			{
				this.Playback.Travel("hurt_left");
			}
			else
			{
				this.Playback.Travel("hurt_right");
			}
		}
	}

	public bool WasHit()
	{
		return this.CurrentState == State.Hurt;
	}

	public void DieAnimationOver()
	{
		this.EmitSignalPlayerDied();
	}


	public void update_look_direction(float direction){
		if (direction > 0)
		{
			Head.FlipH = true;
		}
		else if (direction < 0)
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

	public void wall_jump_state(double delta){
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
		this.update_look_direction(velocity.X);
		velocity.Y += this.Gravity * (float)delta;;
		this.Velocity = velocity;
		if (this.check_wall_jump(velocity.X))
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

	public void PickUp(Item item)
	{
		this.holdingItem?.QueueFree();
		this.holdingItem = item;
		this.PaperSprite.Visible = true;
		this.EmitSignalItemChanged(item);
	}
}
