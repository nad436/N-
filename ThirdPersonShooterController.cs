using Godot;
using System;

public partial class ThirdPersonShooterController : Node
{
    [Export] private Camera3D mainCamera;
    [Export] private Roma player;
    [Export] private CenterContainer crossHairUI;
    [Export] private Control crosshair;
    [Export] private AnimationPlayer animation;

    private bool wasAiming = false;
    private Vector3 defaultCamPos = new Vector3(0.0f, 1.405f, -3.319f);
    private Vector3 AimCamPos = new Vector3(-1.215f, 1.405f, -1.547f);

    public override void _Ready()
    {
        mainCamera.Position = defaultCamPos;
    }

    public override void _Process(double delta)
    {
        bool aiming = Input.IsActionPressed("aim");

        if (aiming != wasAiming) // only trigger when state changes
        {
            if (aiming)
            {
                player.SetMouseSensitivity(1.0f);
                player.SetAimMod(true);
                crossHairUI.SetAiming(true);
                animation.Play("AimIn", customBlend: 0.2f);
            }
            else
            {
                player.SetMouseSensitivity(3.0f);
                player.SetAimMod(false);
                crossHairUI.SetAiming(false);
                animation.Play("AimOut", customBlend: 0.2f);
            }
        }

        crosshair.Visible = aiming;
        wasAiming = aiming;
    }

    public bool canHook()
    {
        return wasAiming && !animation.IsPlaying();
    }

}
