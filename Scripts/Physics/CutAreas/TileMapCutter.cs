using Godot;
using System;

public partial class TileMapCutter : Node2D
{
	// Constants
	private Vector2 RectSize = new Vector2(150.0f, 300.0f);
	//private static readonly Color DefaultColor = new Color(0.8f, 0.0f, 0.224f, 0.392f);
	//private static readonly Color HoverColor   = new Color(0.025f, 0.522f, 0.0f, 0.196f);

	// Onready equivalents
	private Area2D area2D => GetNode<Area2D>("Area2D");
	//private Sprite2D sprite2D => GetNode<Sprite2D>("Sprite2D");

	private uint CurrentLayer = 0;
	private TileMapLayer? CurrentTileMap = null;
	private bool CanCut = false;
	public override void _Ready()
	{
		area2D.BodyEntered += _OnArea2DBodyEntered;
		area2D.BodyExited += _OnArea2DBodyExited;
		//sprite2D.Modulate = DefaultColor;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (this.CanCut && Input.IsActionJustPressed("cut"))
		{
			this.CutTiles(new Rect2(
				this.GlobalPosition - this.RectSize/2.0f, this.RectSize
				));
		}
	}

	private void _OnArea2DBodyEntered(Node body)
	{
		if (body is TileMapLayer tileMap)
		{
		this.CurrentTileMap = (TileMapLayer?)body;
		// Get the collision layer of that physics layer in the TileSet
		uint collisionLayerMask = tileMap.TileSet.GetPhysicsLayerCollisionLayer(0);

		// Check if the collision layer includes the target layer
		if ((collisionLayerMask & this.CurrentLayer) != 0)
		{
			this.CanCut = true;
			//sprite2D.Modulate = HoverColor;
		}
		else
		{
			this.CanCut = false;
			//sprite2D.Modulate = DefaultColor; 
		}
	}
	}

	private void _OnArea2DBodyExited(Node body)
	{
		if (body == this.CurrentTileMap)
		{
		this.CurrentTileMap = null;
		this.CanCut = false;
		//sprite2D.Modulate = DefaultColor;
	}
	}

	public void SetActiveLayers(uint layer)
	{
		this.CurrentLayer = layer;
		if (this.CurrentTileMap is not null)
		{
			this._OnArea2DBodyEntered(this.CurrentTileMap);
		}
	}

	public void CutTiles(Rect2 worldRect)
	{
		if (this.CurrentTileMap == null) return;

		// Convert worldRect to TileMap local coordinates
		Vector2 localPos = this.CurrentTileMap.ToLocal(worldRect.Position);
		Vector2 localEnd = this.CurrentTileMap.ToLocal(worldRect.Position + worldRect.Size);

		// Convert to cell coordinates
		Vector2I minCell = this.CurrentTileMap.LocalToMap(localPos);
		Vector2I maxCell = this.CurrentTileMap.LocalToMap(localEnd);

		// Loop through the cell rectangle
		for (int x = minCell.X; x <= maxCell.X; x++)
		{
			for (int y = minCell.Y; y <= maxCell.Y; y++)
			{
				Vector2I cell = new Vector2I(x, y);

				// Check if there is a tile
				if (this.CurrentTileMap.GetCellSourceId(cell) != -1)
				{
					// Optional: check precise rectangle intersection
					Vector2 tileWorldPos = this.CurrentTileMap.MapToLocal(cell);
					Rect2 tileRect = new Rect2(
						tileWorldPos, this.CurrentTileMap.TileSet.TileSize
					);

					if (worldRect.Intersects(tileRect))
					{
						this.CurrentTileMap.EraseCell(cell);
					}
				}
			}
		}
	}


}
