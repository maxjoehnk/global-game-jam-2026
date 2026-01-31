using GlobalGameJam.Scripts;
using Godot;
using System;

public partial class Enemy : CharacterBody2D, IAssignableLayer
{
	public RayCast2D RayLeft => GetNode<RayCast2D>("Raycasts/RayCastLeft");
	public RayCast2D RayRight => GetNode<RayCast2D>("Raycasts/RayCastRight");

	public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public uint AssignedLayer { 
		get => this.CollisionLayer & Consts.LayerMask;
		set => this.SetCollision(value); 
	}

	public void SetCollision(uint layerMask)
	{
		this.CollisionLayer = (this.CollisionLayer & Consts.NonLayerMask) | layerMask;
		
		uint CollisionValue = (this.CollisionMask & Consts.NonLayerMask) | layerMask;
		this.CollisionMask = CollisionValue;
		this.RayLeft.CollisionMask = CollisionValue;
		this.RayRight.CollisionMask = CollisionValue;
	}

	public bool CheckOnSameLayer(uint OtherMask)
	{
		return (OtherMask & this.AssignedLayer) == this.AssignedLayer;
	}
}
