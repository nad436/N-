using Godot;
using System;

public partial class ProjectileLauncher : Node3D
{
    private PackedScene ProjectileScene;
    private Timer timer;
    private Node3D attack;
    private Camera3D camera3D;

    public override void _Ready()
    {
        ProjectileScene = ResourceLoader.Load<PackedScene>("res://projectile.tscn");
        timer = GetNode<Timer>("Timer");
        //camera3D = GetNode<Camera3D>("GlobalCamera");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (timer.IsStopped())
        {
            if (Input.IsActionPressed("shoot"))
            {
                timer.Start(0.1);
                attack = ProjectileScene.Instantiate<Node3D>();
                AddChild(attack);
                attack.GlobalTransform = GlobalTransform;
            }
        }
    }

    /* private void Shootprojectile()
    {
        Vector2 screenCenter = GetViewport().GetVisibleRect().Size / 2;
        Vector3 rayOrigin = camera3D.ProjectRayOrigin(screenCenter);
        Vector3 rayDir = camera3D.ProjectRayNormal(screenCenter);

        var query = PhysicsRayQueryParameters3D.Create(rayDir, rayOrigin + rayDir * 1000f);
        query.CollisionMask = uint.MaxValue;
        if (GetParent() is PhysicsBody3D parentBody)
            query.Exclude = new Godot.Collections.Array<Rid> { parentBody.GetRid() };


        var spaceState = GetWorld3D().DirectSpaceState;
        var result = spaceState.IntersectRay(query);

        Vector3 targetPoint;
        if (result.Count > 0)
        {
            targetPoint = (Vector3)result["position"];
        }
        else
        {
            
        }
    }
*/
}
