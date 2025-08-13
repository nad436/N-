using Godot;
using System;

public partial class World : Node3D
{
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("quit"))
        {
            GetTree().Quit();
        }
    }

}
