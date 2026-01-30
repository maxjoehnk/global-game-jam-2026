using Godot;

namespace GlobalGameJam.Scripts.BuildingBlocks;

public partial class LevelFinish : Area2D
{
    [Signal]
    public delegate void PlayerReachedGoalEventHandler();

    public override void _Ready()
    {
        this.BodyEntered += _ => this.EmitSignalPlayerReachedGoal();
    }
}