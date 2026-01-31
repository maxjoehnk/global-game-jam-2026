using Godot;
using System;

public partial class WalkingEnemy : Enemy
{
	[Export] public float MovementSpeed = 150f;

	private int direction = 1; // 1 for right, -1 for left
	private Node2D SpriteNode => GetNode<Node2D>("SpriteNode");
	private Area2D PlayerScanArea => GetNode<Area2D>("Area2D");
	private Player? PlayerNode = null;

	public override void _Ready()
	{
		PlayerScanArea.BodyEntered += OnPlayerEntered;
		PlayerScanArea.BodyExited += OnPlayerExited;
	}

	private void OnPlayerEntered(Node body)
	{
		if (body is Player)
		{ 
			Player player_node = (Player)body;
			if (player_node.WasHit()){return;}

			if (this.CheckOnSameLayer(player_node.CollisionMask))
			{
				player_node.Hit(this.GlobalPosition);
			}
			else{
				this.PlayerNode = player_node;
			}
		}
	}

	private void OnPlayerExited(Node body)
	{
		this.PlayerNode = null;
	}

	public override void _PhysicsProcess(double delta)
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

		if (this.PlayerNode != null){
			if (this.PlayerNode.WasHit()){
				this.PlayerNode = null;
			}
			else if (this.CheckOnSameLayer(this.PlayerNode.CollisionMask))
			{
				this.PlayerNode.Hit(this.GlobalPosition);
				this.PlayerNode = null;
			}
		}
	}
}
