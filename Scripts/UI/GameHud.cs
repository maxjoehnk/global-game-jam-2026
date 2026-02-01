using System.Linq;
using GlobalGameJam.Scripts.Core;
using Godot;
using Godot.Collections;

namespace GlobalGameJam.Scripts.UI;

public partial class GameHud : CanvasLayer
{
	private CustomTabBar TabNode => GetNode<CustomTabBar>("CustomTabBar");

	private PauseMenu PauseDialog => GetNode<PauseMenu>("PauseDialog");

	private WonMenu WonDialog => GetNode<WonMenu>("WonDialog");

	public override void _Ready()
	{
		this.ProcessMode = ProcessModeEnum.Always;
		this.PauseDialog.ProcessMode = ProcessModeEnum.WhenPaused;
		this.WonDialog.ProcessMode = ProcessModeEnum.WhenPaused;
		this.WonDialog.Visible = false;
		this.PauseDialog.Visible = false;
	}

	public override void _Input(InputEvent @event)
	{
		if (WonDialog.Visible)
		{
			return;
		}
		if (Input.IsActionJustPressedByEvent(InputAction.Menu, @event) || (Input.IsActionJustPressedByEvent(InputAction.Cancel, @event) && this.PauseDialog.Visible))
		{
			this.PauseDialog.ToggleDialog();
		}
	}

	public void SetActiveLayer(int layerIndex)
	{
		this.TabNode.SetActiveLayer(layerIndex);
	}

	public void SetLayers(int NumberOfLayers, Color[] ColorList)
	{
		for (int i = 0; i < NumberOfLayers; i++)
		{
			this.TabNode.AddLabel($"Mask {i+1}", ColorList[i]);
		}
	}

	public void TogglePauseMenu()
	{
		this.PauseDialog.ToggleDialog();
	}

	public void ShowWonMenu()
	{
		this.WonDialog.ShowDialog();
	}

	public void SetTime(double totalSeconds)
	{
		this.TabNode.SetTimeLabel(totalSeconds);
	}
}
