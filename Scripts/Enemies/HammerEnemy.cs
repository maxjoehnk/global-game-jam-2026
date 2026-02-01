using Godot;
using System;

public enum HammerEnemyState
{
	Walk,
	Hunt,
	Attack,
}


public partial class HammerEnemy : Enemy
{
	[Export] public float MovementSpeed = 80f;
	[Export] public float HuntSpeed = 250f;

	private int direction = 1; // 1 for right, -1 for left
	private Node2D SpriteNode => GetNode<Node2D>("SpriteNode");
	private Area2D PlayerScanArea => GetNode<Area2D>("Area2D");

	private Area2D PlayerAttackArea => GetNode<Area2D>("Area2DAttack");
	private AnimationPlayer AniPlayer => GetNode<AnimationPlayer>("AnimationPlayer");
	private Player? PlayerNode = null;
	private Player? PlayerNodeAttack = null;

	private HammerEnemyState CurrentState = HammerEnemyState.Walk;

	public override void _Ready()
	{
		PlayerScanArea.BodyEntered += OnPlayerEntered;
		PlayerScanArea.BodyExited += OnPlayerExited;

		PlayerAttackArea.BodyEntered += OnPlayerAttack;
		PlayerAttackArea.BodyExited += OnPlayerExitedAttack;
	}

	private void OnPlayerEntered(Node body)
	{
		if (body is Player)
		{ 
			Player player_node = (Player)body;
			if (player_node.WasHit()){return;}
			this.PlayerNode = player_node;
			if (this.CheckOnSameLayer(player_node.CollisionMask))
			{
				this.SetNewState(HammerEnemyState.Hunt);
			}
		}
	}

	private void OnPlayerExited(Node body)
	{
		this.PlayerNode = null;
		if (this.CurrentState == HammerEnemyState.Hunt)
		{
			this.SetNewState(HammerEnemyState.Walk);
		}
	}

	private void OnPlayerAttack(Node body)
	{
		if (body is Player)
		{ 
			Player player_node = (Player)body;
			if (player_node.WasHit()){return;}

			this.PlayerNodeAttack = player_node;
			if (this.CheckOnSameLayer(player_node.CollisionMask))
			{
				this.SetNewState(HammerEnemyState.Attack);
			}
		}
	}

	private void SetNewState(HammerEnemyState NewState)
	{
		if (this.CurrentState != HammerEnemyState.Attack)
		{
			this.CurrentState = NewState;
			if (NewState == HammerEnemyState.Attack)
			{
				this.StartAttack();
			}
			if (NewState == HammerEnemyState.Hunt && this.PlayerNode != null)
			{
				this.direction = Mathf.Sign(
					this.PlayerNode.GlobalPosition.X - this.GlobalPosition.X
				);
			}
		}
	}

	private void OnPlayerExitedAttack(Node body)
	{
		this.PlayerNodeAttack = null;
	}

	private void StartAttack()
	{
		if (this.PlayerNodeAttack != null)
		{
			int dir = Mathf.Sign(
				this.PlayerNodeAttack.GlobalPosition.X - this.GlobalPosition.X
			);
			this.SpriteNode.Scale = new Vector2(
				dir, 1.0f);   
		}
		this.AniPlayer.Play("attack");
	}

	public void AttackPlayer()
	{
		if (this.PlayerNodeAttack != null)
		{
			if (this.CheckOnSameLayer(this.PlayerNodeAttack.CollisionMask))
			{
				this.PlayerNodeAttack.Hit(this.GlobalPosition);
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		switch (this.CurrentState)
		{
			case HammerEnemyState.Walk:
				this.AniPlayer.Play("walk");
				this.WalkState(delta);
				break;
			case HammerEnemyState.Hunt:
				this.AniPlayer.Play("run");
				this.HuntState(delta);
				break;
			case HammerEnemyState.Attack:
				if (!this.AniPlayer.IsPlaying())
				{
					this.CurrentState = HammerEnemyState.Walk;
				}
				break;
		}
	}

	public void WalkState(double delta)
	{
		if (!IsOnFloor())
		{
			Velocity = Velocity with { Y = Velocity.Y + this.Gravity * (float)delta };
		}

		bool CanWalk = direction > 0 ? this.RayRight.IsColliding() : this.RayLeft.IsColliding();
		
		if (!CanWalk)
		{
			direction *= -1;
		}
		if (this.IsOnWall()){
			if (this.GetWallNormal().X * this.direction < 0){
				direction *= -1;
			}
		}
		Velocity = Velocity with { X = MovementSpeed * direction };
		this.SpriteNode.Scale = new Vector2(direction, 1.0f);
		this.MoveAndSlide();
		this.CheckClearPlayer();
	}

	public void HuntState(double delta)
	{
		if (!IsOnFloor())
		{
			Velocity = Velocity with { Y = Velocity.Y + this.Gravity * (float)delta};
		}

		bool CanWalk = direction > 0 ? this.RayRight.IsColliding() : this.RayLeft.IsColliding();
		
		if (!CanWalk)
		{
			direction *= -1;
		}
		if (this.IsOnWall()){
			if (this.GetWallNormal().X * this.direction < 0){
				direction *= -1;
			}
		}
		Velocity = Velocity with { X = this.HuntSpeed * direction};
		this.SpriteNode.Scale = new Vector2(direction, 1.0f);
		this.MoveAndSlide();
		this.CheckClearPlayer();
	}

	public void CheckClearPlayer()
	{
		if (this.PlayerNode != null){
			if (this.PlayerNode.WasHit()){
				this.OnPlayerExited(this.PlayerNode);
			}
			else if (this.CheckOnSameLayer(this.PlayerNode.CollisionMask))
			{
				if (this.PlayerNodeAttack != null)
				{
					this.SetNewState(HammerEnemyState.Attack);
				}
				else if (this.CurrentState == HammerEnemyState.Walk)
				{
					this.SetNewState(HammerEnemyState.Hunt);
				}
			}
			else
			{
				this.SetNewState(HammerEnemyState.Walk);
			}
		}  
	}

	public override void Hit(Projectile? projectile)
	{
		CollisionShape2D CollShape = GetNode<CollisionShape2D>("Polygon2D");
		CollShape.SetDeferred("Disabled", true);
		this.SetPhysicsProcess(false);
		this.PlayerScanArea.QueueFree();
		this.PlayerAttackArea.QueueFree();
		this.PlayerNodeAttack = null;
		this.PlayerNode = null;
		this.AniPlayer.Play("die");
	}
}
