using Godot;
using GlobalGameJam.Scripts;

public partial class Trap : StaticBody2D, IAssignableLayer
{
	private Area2D PlayerScanArea => GetNode<Area2D>("PlayerDetectArea");
	private Player? PlayerNode = null;
	public override void _Ready()
	{   
		this.SetPhysicsProcess(false);
		PlayerScanArea.BodyEntered += OnPlayerEntered;
		PlayerScanArea.BodyExited += OnPlayerExited;
	}

	public uint AssignedLayer { 
		get => this.CollisionLayer & Consts.LayerMask;
		set => this.CollisionLayer = (this.CollisionLayer & Consts.NonLayerMask) | (value & Consts.LayerMask);
	}

	public bool CheckOnSameLayer(uint OtherMask)
	{
		return (OtherMask & this.AssignedLayer) == this.AssignedLayer;
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
		if (this.PlayerNode != null){
			if (this.PlayerNode.WasHit()){
				this.PlayerNode = null;
				this.SetPhysicsProcess(true);
			}
			else if (this.CheckOnSameLayer(this.PlayerNode.CollisionMask))
			{
				this.PlayerNode.Hit(this.GlobalPosition);
				this.PlayerNode = null;
				this.SetPhysicsProcess(true);
			}
		}
	}

}
