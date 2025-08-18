using Godot;
using System;

public partial class CenterContainer : Godot.CenterContainer
{
    [Export] float dotRadius = 1.0f;
    [Export] Color dotColor = new Color(1, 0, 0);
    public bool isAiming { get; set; } = false;
    public override void _Draw()
    {
        if (!isAiming)
        {
            return;
        }
        Vector2 center = Size / 2;
        DrawCircle(center, dotRadius, dotColor);
    }
    public void SetAiming(bool aiming)
    {
        isAiming = aiming;
        QueueRedraw();
    }
}
