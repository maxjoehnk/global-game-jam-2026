using Godot;

namespace GlobalGameJam.Scripts.BuildingBlocks;

public partial class LevelFinish : Area2D, IAssignableLayer
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

			uint assignedLayer = this.AssignedLayer;
			if (assignedLayer == 0)
			{
				// The scene ships with collision_layer = 0; Game._Ready() is what
				// assigns the real layer. Without this guard the check below reads
				// (mask & 0) == 0 and completes the level on any contact, which
				// silently hides whatever kept the assignment from happening.
				GD.PushError($"{this.GetPath()} has no layer assigned, ignoring player contact.");
				return;
			}

			if ((player.CollisionMask & assignedLayer) == assignedLayer)
			{
				this.EmitSignalPlayerReachedGoal();
			}
		};
	}

	// Temporary probe. Object::set consults the script instance before the
	// built-in setter and returns as soon as the script claims the property, so
	// a scripted node that wrongly claims "position" loses it silently. The
	// bridge reaches _Set only when the generated SetGodotClassPropertyValue
	// declined, so seeing this log for "position" narrows where the true is
	// coming from. Remove once the web build behaves.
	public override bool _Set(StringName property, Variant value)
	{
		if (OS.HasFeature("web"))
		{
			GD.Print($"[web-diag] _Set {property} variant={value.VariantType}");
		}

		return false;
	}

	public void LevelDone()
	{
		this.EmitSignalPlayerReachedGoal();
	}

	public uint AssignedLayer
	{
		get => this.CollisionLayer & Consts.LayerMask;
		set => this.CollisionLayer = (this.CollisionLayer & Consts.NonLayerMask) | (value & Consts.LayerMask);
	}
}
