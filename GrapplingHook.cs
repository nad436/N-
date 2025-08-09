using Godot;
using System;
using System.Linq;

public partial class GrappleController : Node3D
{
    [Export] public RayCast3D ray;

    private bool isGrappled;
    private PhysicsBody3D grappletarget;
    private Vector3 grapplePointLocalPosition;

    [Export] public CharacterBody3D player;
    [Export] public float acceleration = 5f;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        ProcessInput();
        ProcessGrapple();

    }

    private void ProcessInput()
    {
        if (Input.IsActionJustPressed("grapple"))
        {
            if (isGrappled)
            {
                isGrappled = false;
                grapplePointLocalPosition = Vector3.Zero;
                grappletarget = null;
            }
            else
            {
                var from = this.GlobalPosition;
                var forward = -this.GlobalBasis.X;
                var to = this.GlobalPosition + forward * 500;
                var raycastResult = GetWorld3D().DirectSpaceState.IntersectRay(PhysicsRayQueryParameters3D.Create(from, to));
                if (raycastResult.Keys.Any())
                {
                    var nodeHit = (Node3D)raycastResult["collider"];
                    if (nodeHit is PhysicsBody3D hitPhysicsBody)
                    {
                        grappletarget = hitPhysicsBody;
                        grapplePointLocalPosition = hitPhysicsBody.ToLocal((Vector3)raycastResult["position"]);
                        isGrappled = true; 
                    }
                }
            }
        }
    }

    public void ProcessGrapple()
    {
        
    }

}
