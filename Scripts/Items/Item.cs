using Godot;
using GlobalGameJam.Scripts;

public partial class Item : RigidBody2D, IAssignableLayer
{
	[Export]
	public PackedScene Projectile { get; set; }
	
	[Export]
	public Texture2D HUDSprite { get; set; }
	
	[Export] public float Velocity { get; set; } = 1000;

	private Area2D Detector => GetNode<Area2D>("Detector");
	
	public override void _Ready()
	{
		this.Detector.BodyEntered += this.OnPlayerEntered;
	}

	public void RemoveFromWorld()
	{
		this.GetParent().RemoveChild(this);
	}

	private void OnPlayerEntered(Node2D body)
	{
		if (body is Player player)
		{
			if ((player.CollisionMask & this.AssignedLayer) == this.AssignedLayer)
			{
				player.PickUp(this);
				this.CallDeferred(nameof(this.RemoveFromWorld));
			}
		}
	}

	public void Throw(Node parent, Vector2 globalPosition, bool lookingRight)
	{
		Vector2 linearVelocity = new(lookingRight ? this.Velocity : -1 * this.Velocity, 0);
		RigidBody2D projectile = this.Projectile.Instantiate<RigidBody2D>();
		projectile.GlobalPosition = globalPosition;
		projectile.LinearVelocity = linearVelocity;
		parent.AddChild(projectile);
		this.QueueFree();
	}

	public uint AssignedLayer
	{
		get => this.CollisionLayer & Consts.LayerMask;
		set => this.CollisionLayer = (this.CollisionLayer & Consts.NonLayerMask) | (value & Consts.LayerMask);
	}
}
