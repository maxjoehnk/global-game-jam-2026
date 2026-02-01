using System.Linq;
using GlobalGameJam.Scripts;
using GlobalGameJam.Scripts.BuildingBlocks;
using Godot;
using GlobalGameJam.Scripts.Core;
using GlobalGameJam.Scripts.UI;
using Godot.Collections;
using System.ComponentModel.DataAnnotations;
using System;

public partial class Game : Node2D
{
	private static Lazy<AudioStreamPlaylist> LevelLoadedQuotes = new(() => GD.Load<AudioStreamPlaylist>("res://Assets/Sounds/Quotes/LevelLoadedQuote.tres"));
	
	[Export]
	public int InitialLayer;

	private const int PhysicsBaseLayer = 8;
	private const int CutBaseLayer = 5;
	private const float LayerScaling = 0.025f;

	private Node2D LayerContainer => GetNode<Node2D>("Layers");
	private Player Player => GetNode<Player>("Player");
	private GameHud Hud => GetNode<GameHud>("UI/HUD");
	private Marker2D RespawnMarker => GetNode<Marker2D>("RespawnPoint");

	private int LayerCount => LayerContainer.GetChildCount();

	private int activeLayerIndex;

	private Node2D ActiveLayer => LayerContainer.GetChild<Node2D>(this.activeLayerIndex);
	private uint ActivePhysicsLayer => (uint)1 << (PhysicsBaseLayer + this.activeLayerIndex);


	private static readonly Color[] LayerColorList =
	{
		new Color(0.996f, 0.0f, 0.246f),
		new Color(0.2f, 0.256f, 1.0f),
		new Color(0.264f, 0.494f, 0.0f),
		new Color(0.825f, 0.042f, 0.602f, 1.0f),
		new Color(0.68f, 0.46f, 0.0f),
		new Color(0.08f, 0.561f, 0.734f),
		new Color(0.77f, 0.26f, 0.0f),
		new Color(0.333f, 0.747f, 0.559f),
	};

	public override void _Ready()
	{
		int physicsLayer = PhysicsBaseLayer;
		Array<Node> layers = this.LayerContainer.GetChildren();
		this.Hud.SetLayers(layers.Count, LayerColorList);
		foreach (Node2D layer in layers)
		{
			uint layerMask = (uint)1 << physicsLayer;
			foreach (IAssignableLayer assignableLayer in layer.FindChildren("*").Where(c => c is IAssignableLayer).Cast<IAssignableLayer>())
			{
				assignableLayer.AssignedLayer = layerMask;
			}
			
			foreach (PhysicsBody2D physicsBody2D in layer.FindChildren("*", type: nameof(PhysicsBody2D))
						 .Where(c => c is PhysicsBody2D)
						 .Cast<PhysicsBody2D>())
			{
				physicsBody2D.CollisionLayer = (physicsBody2D.CollisionLayer & Consts.NonLayerMask) | layerMask;		
			}

			if (layer is TileMapLayer)
			{
				ApplyMaskLayerToTileMapLayer((TileMapLayer)layer, layerMask);
			}
			
			foreach (TileMapLayer tileMapLayer in layer.GetChildren().Where(c => c is TileMapLayer).Cast<TileMapLayer>())
			{
				ApplyMaskLayerToTileMapLayer(tileMapLayer, layerMask);
			}
			physicsLayer++;
		}
		this.activeLayerIndex = this.InitialLayer;
		this.UpdateActiveLayer();

		this.Player.PlayerDied += this.RespawnPlayer;
		this.RespawnMarker.GlobalPosition = this.Player.GlobalPosition;
		foreach (LevelFinish finish in this.GetTree().GetNodesInGroup("LevelExits").Cast<LevelFinish>())
		{
			finish.PlayerReachedGoal += this.OnPlayerReachedGoal;
		}

		this.PlayLevelLoadedQuote();
	}

	private void PlayLevelLoadedQuote()
	{
		AudioStreamPlaylist levelLoadedQuotes = LevelLoadedQuotes.Value;
		int quoteIndex = GD.RandRange(0, levelLoadedQuotes.StreamCount - 1);
		AudioStreamPlayer audioStreamPlayer = this.GetNode<AudioStreamPlayer>("LevelLoadedQuote");
		audioStreamPlayer.Stream = levelLoadedQuotes.GetListStream(quoteIndex);
		audioStreamPlayer.Play();
	}

	private static void ApplyMaskLayerToTileMapLayer(TileMapLayer tileMapLayer, uint physicsLayer)
	{
		TileSet uniqueTileSet = (tileMapLayer.TileSet.Duplicate() as TileSet)!;
		uint tileMapCollisionLayer = uniqueTileSet.GetPhysicsLayerCollisionLayer(0) & Consts.NonLayerMask;
		uniqueTileSet.SetPhysicsLayerCollisionLayer(0, tileMapCollisionLayer | physicsLayer);
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
			layer.Modulate = i == this.activeLayerIndex ? Color.FromHsv(0, 0, 1) : Color.FromHsv(0, 0, 1, 0.2f);
			float scaling_value = 1.0f + LayerScaling*(i - this.activeLayerIndex);
			layer.Scale = new Vector2(scaling_value, scaling_value);
			if (i != this.activeLayerIndex)
			{
				layer.Modulate *= LayerColorList[i].Lerp(new Color(1.0f,1.0f,1.0f), 0.15f);
			}
		}
		Player.SetActiveCollisionLayer(this.ActivePhysicsLayer);
		this.Hud.SetActiveLayer(this.activeLayerIndex);
	}

	private void RespawnPlayer(){
		this.activeLayerIndex = this.InitialLayer;
		this.UpdateActiveLayer();
		this.Player.GlobalPosition = this.RespawnMarker.GlobalPosition;
		this.Player.Reset();
	}

	private void OnPlayerReachedGoal()
	{
		this.Hud.ShowWonMenu();
	}
}
