using Godot;
using System;

public partial class LevelButton : Button
{
    public string LevelName
    {
        set => this.Text = value;
    }
}