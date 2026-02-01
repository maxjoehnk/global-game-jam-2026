using Godot;
using System;
using GlobalGameJam.Scripts;

public partial class Schredder : Trap
{
	[Export]
	private GlobalGameJam.Scripts.BuildingBlocks.LevelFinish? level_finish_node = null;

	private AnimationPlayer AniPlayer => GetNode<AnimationPlayer>("AnimationPlayer");

	public override void _Ready()
	{   
		base._Ready();
		if (this.level_finish_node != null)
		{
			this.level_finish_node.Visible = false;
			this.level_finish_node.GlobalPosition = new Vector2(100000.0f,10000.0f);
		}
	}

	public void LevelDone()
	{
		if (this.level_finish_node != null)
			{
				this.level_finish_node.LevelDone();
		}
	}

	public override void OnPlayerEntered(Node body)
	{
		if (body is Player)
		{ 
			Player player_node = (Player)body;
			if (this.CheckOnSameLayer(player_node.CollisionMask) && player_node.HasAntrag)
			{
				player_node.GotAntrag(false);
				this.AniPlayer.Play("shred");
			}
		}
	}

}
