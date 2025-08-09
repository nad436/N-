using Godot;
using System;

public partial class Camera3d : Camera3D
{
    [Export] public float LeanAmount { get; set; } = 0.3f;
    [Export] public float LeanTilt { get; set; } = 10f;
    [Export] public float LeanSpeed { get; set; } = 5f;

    private Vector3 targetOffset = Vector3.Zero;
    private Vector3 currentOffset = Vector3.Zero;
    private float targetRotation = 0f;
    private float currentRotation = 0f;
    private Vector3 originalPosition;
    private float originalRotationZ;

    private Area3D leftArea;
    private Area3D rightArea;


    public override void _Ready()
    {
        leftArea = GetParent().GetNode<Area3D>("PeekDetectorLeft");
        rightArea = GetParent().GetNode<Area3D>("PeekDetectorRight");
        originalPosition = Position;
        originalRotationZ = RotationDegrees.Z;
    }

    public override void _PhysicsProcess(double delta)
    {
        bool canLeanLeft = leftArea.GetOverlappingBodies().Count > 1;
        bool canLeanRight = rightArea.GetOverlappingBodies().Count > 1;

        if (Input.IsActionPressed("lean_left") && canLeanLeft)
        {
            targetOffset.X = -LeanAmount;
            targetRotation = LeanTilt;
           

        }
        else if (Input.IsActionPressed("lean_right") && canLeanRight)
        {
            targetOffset.X = LeanAmount;
            targetRotation = -LeanTilt;
            

        }
        else
        {
            targetOffset.X = 0f;
            targetRotation = 0f;
            
        }

        float deltaf = (float)delta;

        // Smooth interpolation
        currentOffset = currentOffset.Lerp(targetOffset, LeanSpeed * deltaf);
        currentRotation = Mathf.Lerp(currentRotation, targetRotation, LeanSpeed * deltaf);

        Vector3 leanOffset = Transform.Basis.X * currentOffset.X;
            Position = originalPosition + leanOffset;
        
        // Apply leaning
        
        RotationDegrees = new Vector3(RotationDegrees.X, RotationDegrees.Y, originalRotationZ + currentRotation);
    }
    
}
