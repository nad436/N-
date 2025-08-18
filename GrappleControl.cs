using Godot;
using System;

public partial class GrappleControl : Node
{
    [Export] RayCast3D ray;
    [Export] float restLenght = 2.0f;
    [Export] float stiffness = 10.0f;
    [Export] float damping = 1.0f;
    [Export] CharacterBody3D player;
    [Export] Camera3D camera;
    [Export] Node3D rope;
    [Export] ThirdPersonShooterController aimControl;

    private Vector3 target;
    private bool launched = false;

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("hook") && aimControl.canHook())
        {
            Launch();
        }
        if (Input.IsActionJustReleased("hook"))
        {
            Retrack();
        }
        if (launched)
        {
            handleGrapple((float)delta);
        }

        updateRope();
    }

    public void Launch()
    {
        Vector2 screenCenter = GetViewport().GetVisibleRect().Size / 2;

        Vector3 camOrigin = camera.ProjectRayOrigin(screenCenter);
        Vector3 camDir = camera.ProjectRayNormal(screenCenter);

        float maxDist = 100f;
        Vector3 rayEnd = camOrigin + camDir * maxDist;

        var spaceState = camera.GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(camOrigin, rayEnd);
        var result = spaceState.IntersectRay(query);

        if (result.Count > 0)
        {
            target = (Vector3)result["position"];
            launched = true;
            GD.Print("Hook target: ", target);
        }
    }
    public void Retrack()
    {
        launched = false;
    }
    public void handleGrapple(float delta)
    {
        var targetDir = player.GlobalPosition.DirectionTo(target);
        var targetDist = player.GlobalPosition.DistanceTo(target);

        var displacement = targetDist - restLenght;

        if (displacement > 0)
        {
            var springForceMagnitude = stiffness * displacement;
            var springForce = targetDir * springForceMagnitude;

            var velDot = player.Velocity.Dot(targetDir);
            var dampingForce = -damping * velDot * targetDir;

            var force = springForce + dampingForce;

            player.Velocity += force * delta;
        }

        player.MoveAndSlide();
    }
    public void updateRope()
    {
        if (!launched)
        {
            rope.Visible = false;
            return;
        }
        rope.Visible = true;

        var dis = player.GlobalPosition.DistanceTo(target);

        rope.LookAt(target);
        rope.Scale = new Vector3(1, 1, dis);
    }


}
