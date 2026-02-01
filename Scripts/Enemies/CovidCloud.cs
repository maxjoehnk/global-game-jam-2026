using Godot;
using System;

public partial class CovidCloud : Enemy
{
	
	private Area2D PlayerScanArea => GetNode<Area2D>("Area2D");
	private Player? PlayerNode = null;

	public override void _Ready()
	{
		PlayerScanArea.BodyEntered += OnPlayerEntered;
		PlayerScanArea.BodyExited += OnPlayerExited;
		this.SetPhysicsProcess(false);
	}

	private void OnPlayerEntered(Node body)
	{
		if (body is Player)
		{ 
			Player player_node = (Player)body;
			if (player_node.WasHit()){return;}
			this.PlayerNode = player_node;
			if (this.CheckOnSameLayer(player_node.CollisionMask) && !player_node.HasMask())
			{
				this.PlayerNode.Hit(this.GlobalPosition);
				this.PlayerNode = null;
			}
			else
			{
				this.SetPhysicsProcess(true);
			}
		}
	}

	private void OnPlayerExited(Node body)
	{
		this.PlayerNode = null;
		this.SetPhysicsProcess(false);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (this.PlayerNode != null)
		{
			if (this.PlayerNode.WasHit()){
				this.PlayerNode = null;
			}
			else if (this.CheckOnSameLayer(this.PlayerNode.CollisionMask) && !this.PlayerNode.HasMask())
			{
				this.PlayerNode.Hit(this.GlobalPosition);
				this.PlayerNode = null;
			}
		}
		else
		{
			this.SetPhysicsProcess(false);
		}
	}
}
