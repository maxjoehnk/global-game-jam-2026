using System.Linq;
using GlobalGameJam.Scripts;
using GlobalGameJam.Scripts.BuildingBlocks;
using Godot;
using GlobalGameJam.Scripts.Core;
using GlobalGameJam.Scripts.UI;
using Godot.Collections;
using System.ComponentModel.DataAnnotations;
using System;
using System.Numerics;

public partial class Game : Node2D
{
	private static Lazy<AudioStreamPlaylist> LevelLoadedQuotes = new(() => GD.Load<AudioStreamPlaylist>("res://Assets/Sounds/Quotes/LevelLoadedQuote.tres"));

	[Export]
	public int InitialLayer;

	[Export] public Godot.Vector2 CameraBoundsX = new Godot.Vector2(0.0f, 1920.0f);
	[Export] public Godot.Vector2 CameraBoundsY = new Godot.Vector2(0.0f, 1080.0f);

	private const int PhysicsBaseLayer = 8;
	private const int CutBaseLayer = 5;
	private const float LayerScaling = 0.0f;

	private Node2D LayerContainer => GetNode<Node2D>("Layers");
	private Player Player => GetNode<Player>("Player");
	private Camera2D PlayerCam => GetNode<Camera2D>("Player/Camera2D");
	private GameHud Hud => GetNode<GameHud>("UI/HUD");
	private Marker2D RespawnMarker => GetNode<Marker2D>("RespawnPoint");

	private int LayerCount => LayerContainer.GetChildCount();

	private int activeLayerIndex;

	private Node2D ActiveLayer => LayerContainer.GetChild<Node2D>(this.activeLayerIndex);
	private uint ActivePhysicsLayer => (uint)1 << (PhysicsBaseLayer + this.activeLayerIndex);


	private static readonly Color[] LayerColorList =
	{
		Color.FromHtml("3b98cc"),
		Color.FromHtml("d83a67"),
		Color.FromHtml("009245"),
		Color.FromHtml("baa45d"),
		Color.FromHtml("5151b7"),
		Color.FromHtml("5a93a0"),
		Color.FromHtml("f2e29e"),
		Color.FromHtml("847356"),
	};

	private double LevelTime = 0.0;

	public override void _Ready()
	{
		// Before anything below mutates the tree, in particular RespawnPoint.
		this.LogWebDiagnostics();

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
		// Set camera limits
		this.PlayerCam.LimitLeft = (int)this.CameraBoundsX.X;
		this.PlayerCam.LimitRight = (int)this.CameraBoundsX.Y;
		this.PlayerCam.LimitTop = (int)this.CameraBoundsY.X;
		this.PlayerCam.LimitBottom = (int)this.CameraBoundsY.Y;
		this.Player.ResetHeight = this.CameraBoundsY.Y + 100.0f;
		this.Player.ItemChanged += this.Hud.UpdateItem;

		this.PlayLevelLoadedQuote();
	}

	// Temporary: instanced nodes lose their position overrides on web while plain
	// nodes keep theirs. Every node observed broken so far is both instanced and
	// C# scripted, so log both attributes for every Node2D to tell which one
	// actually predicts the failure. SceneFilePath is non-empty exactly on the
	// root of an instanced scene. Positions print as ints to keep float
	// formatting out of it. Remove once the web build behaves.
	private void LogWebDiagnostics()
	{
		if (!OS.HasFeature("web"))
		{
			return;
		}

		int logged = 0;
		LogNode(this, ref logged);

		static void LogNode(Node node, ref int logged)
		{
			if (logged >= 40)
			{
				return;
			}

			if (node is Node2D node2D)
			{
				logged++;
				bool instanced = !string.IsNullOrEmpty(node.SceneFilePath);
				bool scripted = node.GetScript().VariantType != Variant.Type.Nil;
				Godot.Vector2 local = node2D.Position;
				GD.Print(
					$"[web-diag] {node.Name} type={node.GetType().Name} " +
					$"instanced={instanced} scripted={scripted} " +
					$"pos=({(int)local.X},{(int)local.Y})");
			}

			foreach (Node child in node.GetChildren())
			{
				LogNode(child, ref logged);
			}
		}
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
			float scaling_value = 1.0f + LayerScaling * (i - this.activeLayerIndex);
			layer.Scale = new Godot.Vector2(scaling_value, scaling_value);
			if (i != this.activeLayerIndex)
			{
				layer.Modulate *= LayerColorList[i].Lerp(new Color(1.0f, 1.0f, 1.0f), 0.25f);
			}
		}
		Player.SetActiveCollisionLayer(this.ActivePhysicsLayer);
		this.Hud.SetActiveLayer(this.activeLayerIndex);
	}

	private void RespawnPlayer()
	{
		this.activeLayerIndex = this.InitialLayer;
		this.UpdateActiveLayer();
		this.Player.GlobalPosition = this.RespawnMarker.GlobalPosition;
		this.Player.Reset();
	}

	private void OnPlayerReachedGoal()
	{
		this.Hud.ShowWonMenu(this.LevelTime);
		SceneManager.Instance.FinishedLevel(this.LevelTime);
	}

	public override void _Process(double delta)
	{
		this.LevelTime += delta;
		this.Hud.SetTime(this.LevelTime);
	}
}
