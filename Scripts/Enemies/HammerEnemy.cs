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

			if (this.CheckOnSameLayer(player_node.CollisionMask))
			{
				this.CurrentState = HammerEnemyState.Hunt;
			}
			else{
				this.PlayerNode = player_node;
			}
		}
	}

	private void OnPlayerExited(Node body)
	{
		this.PlayerNode = null;
		if (this.CurrentState == HammerEnemyState.Hunt)
		{
			this.CurrentState = HammerEnemyState.Walk;
		}
	}

	private void OnPlayerAttack(Node body)
	{
		if (body is Player)
		{ 
			Player player_node = (Player)body;
			if (player_node.WasHit()){return;}

			if (this.CheckOnSameLayer(player_node.CollisionMask))
			{
				this.CurrentState = HammerEnemyState.Hunt;
			}
			else{
				this.PlayerNode = player_node;
			}
		}
	}

	private void OnPlayerExitedAttack(Node body)
	{
		this.PlayerNodeAttack = null;
	}

	private void StartAttack()
	{
		this.CurrentState = HammerEnemyState.Attack;
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
				this.AniPlayer.Play("walk");
				this.HuntState(delta);
				break;
			case HammerEnemyState.Attack:
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
			this.CurrentState = HammerEnemyState.Walk;

		}
		if (this.IsOnWall()){
			if (this.GetWallNormal().X * this.direction < 0){
				direction *= -1;
				this.CurrentState = HammerEnemyState.Walk;
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
					this.StartAttack();
				}
				else if (this.CurrentState == HammerEnemyState.Walk)
				{
					this.CurrentState = HammerEnemyState.Hunt;
				}
			}
			else
			{
				this.CurrentState = HammerEnemyState.Walk;
			}
		}  
	}
}
