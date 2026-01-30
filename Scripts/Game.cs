using System.Linq;
using Godot;
using GlobalGameJam.Scripts.Core;

public partial class Game : Node2D
{
	private const int PhysicsBaseLayer = 8;

	private Node2D LayerContainer => GetNode<Node2D>("Layers");
	private Player Player => GetNode<Player>("Player");
	private Label LayerIndicator => GetNode<Label>("LayerIndicator");

	private int LayerCount => LayerContainer.GetChildCount();

	private int activeLayerIndex;

	private Node2D ActiveLayer => LayerContainer.GetChild<Node2D>(this.activeLayerIndex);
	private uint ActivePhysicsLayer => (uint)1 << (PhysicsBaseLayer + this.activeLayerIndex);

	public override void _Ready()
	{
		this.UpdateActiveLayer();
		int physicsLayer = PhysicsBaseLayer;
		foreach (Node child in this.LayerContainer.GetChildren())
		{
			foreach (PhysicsBody2D physicsBody2D in child.GetChildren(includeInternal: true)
						 .Where(c => c is PhysicsBody2D)
						 .Cast<PhysicsBody2D>())
			{
				physicsBody2D.CollisionLayer = (uint)1 << physicsLayer;
			}
			physicsLayer++;
		}
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed(InputAction.LayerUp))
		{
			GD.Print("Next Layer");
			this.NextLayer();
		}

		if (Input.IsActionJustPressed(InputAction.LayerDown))
		{
			GD.Print("Previous Layer");
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
			// layer.ProcessMode = i == this.activeLayerIndex ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
			layer.Visible = i == this.activeLayerIndex;
		}

		Player.SetActiveCollisionLayer(this.ActivePhysicsLayer);
		LayerIndicator.Text = $"Layer {this.activeLayerIndex + 1}";
	}
}
