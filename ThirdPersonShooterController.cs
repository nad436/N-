using Godot;
using System;

public partial class ThirdPersonShooterController : Node
{
    [Export] private Camera3D aCamera;
    [Export] private Camera3D bCamera;
    [Export] private Roma player;

    public override void _Ready()
    {
        aCamera.Current = false;
        bCamera.Current = true;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("aim"))
        {
            aCamera.Current = true;
            bCamera.Current = false;
            player.SetMouseSensitivity(1.0f);
            player.SetAimMod(true);
        }
        else
        {
            aCamera.Current = false;
            bCamera.Current = true;
            player.SetMouseSensitivity(3.0f);
            player.SetAimMod(false);
        }
    }

}
