using Godot;
using System;

public partial class LevelBackground : ColorRect
{
	
	public void SetBorderColor(Color NewColor)
	{
		this.SelfModulate = NewColor;
	}
}
