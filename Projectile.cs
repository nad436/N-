using Godot;
using System;

public partial class Projectile : RayCast3D
{
    [Export] float speed = 50.0f;

    public override void _PhysicsProcess(double delta)
    {
        Position += GlobalBasis * Vector3.Forward * speed * (float)delta;
        TargetPosition = Vector3.Forward * speed * (float)delta;
        ForceRaycastUpdate();
        var collider = GetCollider();
        if (IsColliding())
        {
            GlobalPosition = GetCollisionPoint();
            SetPhysicsProcess(false);
        }

    }
    public void Cleanup()
    {
        QueueFree();
    }

}
