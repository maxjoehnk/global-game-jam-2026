using System.Linq;
using GlobalGameJam.Scripts.BuildingBlocks;
using Godot;
using GlobalGameJam.Scripts.Core;
using GlobalGameJam.Scripts.UI;
using Godot.Collections;

public partial class Game : Node2D
{
	[Export]
	public int InitialLayer;

	private const int PhysicsBaseLayer = 8;
	private const int CutBaseLayer = 5;
	private const uint NonPhysicsLayerMask = 0b11111111;
	
	private Node2D LayerContainer => GetNode<Node2D>("Layers");
	private Player Player => GetNode<Player>("Player");
	private GameHud Hud => GetNode<GameHud>("UI/HUD");
	private Marker2D RespawnMarker => GetNode<Marker2D>("RespawnPoint");

	private int LayerCount => LayerContainer.GetChildCount();

	private int activeLayerIndex;

	private Node2D ActiveLayer => LayerContainer.GetChild<Node2D>(this.activeLayerIndex);
	private uint ActivePhysicsLayer => (uint)1 << (PhysicsBaseLayer + this.activeLayerIndex);

	public override void _Ready()
	{
		int physicsLayer = PhysicsBaseLayer;
		Array<Node> layers = this.LayerContainer.GetChildren();
		this.Hud.SetLayers(new Array<string>(layers.Select(l => l.Name.ToString())));
		foreach (Node layer in layers)
		{
			foreach (PhysicsBody2D physicsBody2D in layer.FindChildren("*", type: nameof(PhysicsBody2D))
						 .Where(c => c is PhysicsBody2D)
						 .Cast<PhysicsBody2D>())
			{
				physicsBody2D.CollisionLayer = (physicsBody2D.CollisionLayer & NonPhysicsLayerMask) | (uint)1 << physicsLayer;
			}

			if (layer is TileMapLayer)
			{
				ApplyMaskLayerToTileMapLayer((TileMapLayer)layer, physicsLayer);
			}
			
			foreach (TileMapLayer tileMapLayer in layer.GetChildren().Where(c => c is TileMapLayer).Cast<TileMapLayer>())
			{
				ApplyMaskLayerToTileMapLayer(tileMapLayer, physicsLayer);
			}

			physicsLayer++;
		}
		this.activeLayerIndex = this.InitialLayer;
		this.UpdateActiveLayer();

		this.Player.PlayerDied += this.RespawnPlayer;
		foreach (LevelFinish finish in this.GetTree().GetNodesInGroup("LevelExits").Cast<LevelFinish>())
		{
			finish.PlayerReachedGoal += this.OnPlayerReachedGoal;
		}
	}

	private static void ApplyMaskLayerToTileMapLayer(TileMapLayer tileMapLayer, int physicsLayer)
	{
		TileSet uniqueTileSet = (tileMapLayer.TileSet.Duplicate() as TileSet)!;
		uint tileMapCollisionLayer = uniqueTileSet.GetPhysicsLayerCollisionLayer(0) & NonPhysicsLayerMask;
		uniqueTileSet.SetPhysicsLayerCollisionLayer(0, tileMapCollisionLayer | (uint)1 << physicsLayer);
		tileMapLayer.TileSet = uniqueTileSet;
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressedByEvent(InputAction.LayerUp, @event))
		{
			this.NextLayer();
		}

		if (Input.IsActionJustPressedByEvent(InputAction.LayerDown, @event))
		{
			this.PreviousLayer();
		}
	}

	private void NextLayer()
	{
		this.activeLayerIndex += 1;
		if (this.activeLayerIndex >= this.LayerCount)
		{
			this.activeLayerIndex = 0;
		}

		this.UpdateActiveLayer();
	}

	private void PreviousLayer()
	{
		this.activeLayerIndex -= 1;
		if (this.activeLayerIndex < 0)
		{
			this.activeLayerIndex = this.LayerCount - 1;
		}

		this.UpdateActiveLayer();
	}

	private void UpdateActiveLayer()
	{
		for (int i = 0; i < this.LayerContainer.GetChildren().Count; i++)
		{
			Node2D layer = this.LayerContainer.GetChild<Node2D>(i);
			layer.Modulate = i == this.activeLayerIndex ? Color.FromHsv(0, 0, 1) : Color.FromHsv(0, 0, 1, 0.25f);
		}
		Player.SetActiveCollisionLayer(this.ActivePhysicsLayer);
		this.Hud.SetActiveLayer(this.activeLayerIndex);
	}

	private void RespawnPlayer(){
		this.activeLayerIndex = this.InitialLayer;
		this.UpdateActiveLayer();
		this.Player.GlobalPosition = this.RespawnMarker.GlobalPosition;
	}

	private void OnPlayerReachedGoal()
	{
		this.Hud.ShowWonMenu();
	}
}
