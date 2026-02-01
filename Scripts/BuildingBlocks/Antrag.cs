using Godot;
using System;

namespace GlobalGameJam.Scripts.BuildingBlocks;
public partial class Antrag : Area2D, IAssignableLayer
{
	[Signal]
	public delegate void PlayerReachedGoalEventHandler();

	public override void _Ready()
	{
		this.BodyEntered += body =>
		{
			if (body is not Player player)
			{
				return;
			}

			if ((player.CollisionMask & this.AssignedLayer) == this.AssignedLayer)
			{
				player.GotAntrag(true);
				this.QueueFree();
			}
		};
	}

	public uint AssignedLayer
	{
		get => this.CollisionLayer & Consts.LayerMask;
		set => this.CollisionLayer = (this.CollisionLayer & Consts.NonLayerMask) | (value & Consts.LayerMask);
	}
}
