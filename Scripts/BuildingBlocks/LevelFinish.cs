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

            if ((player.CollisionMask & this.AssignedLayer) == this.AssignedLayer)
            {
                this.EmitSignalPlayerReachedGoal();
            }
        };
    }

    public uint AssignedLayer
    {
        get => this.CollisionLayer & Consts.LayerMask;
        set => this.CollisionLayer = (this.CollisionLayer & Consts.NonLayerMask) | (value & Consts.LayerMask);
    }
}