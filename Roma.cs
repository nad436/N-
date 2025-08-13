using Godot;
using System;

public partial class Roma : CharacterBody3D
{
    [Export] float runSpeed { get; set; } = 5f;
    [Export] float angularVelocity { get; set; } = 10f;
    [Export] float Speed { get; set; } = 5f;
    [Export] float jumpImpulse { get; set; } = 20f;
    [Export] int dullGravity { get; set; } = 20;
    [Export] int Gravity { get; set; } = 20;

    MeshInstance3D mesh;
    Vector3 moveDir;
    Vector3 vel;

    [Export] float acceleration { get; set; } = 20f;
    [Export] float deceleration { get; set; } = 20f;
    [Export] float maxSpeed { get; set; } = 20f;

    private Label speedLabel;

    // Camera motion
    private float pitch = 0.0f;
    private float yaw = 0.0f;
    [Export] float mouseSensitivity { get; set; } = 3.0f;
    [Export] float gamepadCameraSensitivity { get; set; } = 3.0f;

    // Dash
    bool isDashing = false;
    [Export] float dashTime = 0.2f;
    [Export] float dashTimer = 0f;
    [Export] float dashSpeed = 30f;
    private Vector3 dashDirection = Vector3.Zero;

    // Climbing
    private bool isClimbing = false;
    [Export] float climbingJump { get; set; } = 10f;
    [Export] float climbingSpeed { get; set; } = 4f;
    RayCast3D wallCheck;
    RayCast3D stillOnWallCheck;

    public override void _Ready()
    {
        mesh = GetNode<MeshInstance3D>("MeshInstance3D");
        Input.MouseMode = Input.MouseModeEnum.Captured;

        speedLabel = GetNode<Label>("Control/Label");

        yaw = Rotation.Y;
        pitch = GetNode<Node3D>("CameraPivot").Rotation.X;

        wallCheck = GetNode<RayCast3D>("wallCheck");
        stillOnWallCheck = GetNode<RayCast3D>("stillOnWallCheck");
    }

    public override void _Process(double delta)
    {
        
        /*float currentSpeed = new Vector3(Velocity.X, 0, Velocity.Z).Length();
        speedLabel.Text = $"Speed: {Mathf.Round(currentSpeed)}";*/

        // Gamepad look
        float lookX = Input.GetActionStrength("came_rotation_right") - Input.GetActionStrength("came_rotation_left");
        float lookY = Input.GetActionStrength("came_rotation_forward") - Input.GetActionStrength("came_rotation_back");

        if (Mathf.Abs(lookX) > 0.1f || Mathf.Abs(lookY) > 0.1f)
        {
            yaw -= lookX * gamepadCameraSensitivity * (float)delta;
            pitch -= lookY * gamepadCameraSensitivity * (float)delta;
            pitch = Mathf.Clamp(pitch, Mathf.DegToRad(-30f), Mathf.DegToRad(60f));

            Rotation = new Vector3(0, yaw, 0);
            Node3D camera = GetNode<Node3D>("CameraPivot");
            if (camera != null)
            {
                camera.Rotation = new Vector3(pitch, 0, 0);
            }
        }

        // Rotate mesh toward movement
        if (moveDir != Vector3.Zero && !isClimbing)
        {
            if (mesh != null)
            {
                var r = mesh.Rotation;
                r.Y = Mathf.LerpAngle(mesh.Rotation.Y, Mathf.Atan2(-moveDir.X, -moveDir.Z), angularVelocity * (float)delta);
                mesh.Rotation = r;
            }
        }
            else if (moveDir != Vector3.Zero && isClimbing)
            {
            if (mesh != null)
            {
                var r = mesh.Rotation;
                r.Y = -(Mathf.Atan2(wallCheck.GetCollisionNormal().Z, wallCheck.GetCollisionNormal().X) - Mathf.Pi / 2);
                mesh.Rotation = r;
            }
            }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Get input direction
        var inputDir = Vector3.Zero;
        if (Input.IsActionPressed("move_left")) inputDir.X += 1.0f;
        if (Input.IsActionPressed("move_right")) inputDir.X -= 1.0f;
        if (Input.IsActionPressed("move_back")) inputDir.Z += 1.0f;
        if (Input.IsActionPressed("move_forward")) inputDir.Z -= 1.0f;
        inputDir = inputDir.Normalized();

        // Move direction in world space
        Vector3 forward = -Transform.Basis.Z;
        Vector3 right = Transform.Basis.X;
        moveDir = (right * inputDir.X + forward * inputDir.Z).Normalized();

        vel = Velocity;

        // Handle climbing
        Climbing();

        // Dash
        if (!isDashing && Input.IsActionJustPressed("dash") && moveDir != Vector3.Zero)
        {
            isDashing = true;
            dashTimer = dashTime;
            dashDirection = moveDir;
        }

        if (isDashing)
        {
            vel = dashDirection * dashSpeed;
            dashTimer -= (float)delta;
            if (dashTimer <= 0) isDashing = false;
        }
        else if (isClimbing)
        {
            // Velocity handled in Climbing()
        }
        else
        {
            // Horizontal movement
            Vector3 horizontalVelocity = new Vector3(vel.X, 0, vel.Z);
            horizontalVelocity = moveDir != Vector3.Zero
                ? horizontalVelocity.MoveToward(moveDir * maxSpeed, acceleration * (float)delta)
                : horizontalVelocity.MoveToward(Vector3.Zero, deceleration * (float)delta);

            vel.X = horizontalVelocity.X;
            vel.Z = horizontalVelocity.Z;

            // Gravity
            if (!IsOnFloor()) vel.Y -= dullGravity * (float)delta;
            else if (vel.Y < 0) vel.Y = 0;

            // Jump
            if (IsOnFloor() && Input.IsActionPressed("jump"))
            {
                vel.Y = jumpImpulse;
                dullGravity = 20;
            }
            if (Input.IsActionJustReleased("jump"))
                dullGravity = 40;
        }

        Velocity = vel;
        MoveAndSlide();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            yaw -= motion.Relative.X / 1000f * mouseSensitivity;
            pitch -= motion.Relative.Y / 1000f * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, Mathf.DegToRad(-30f), Mathf.DegToRad(10f));

            Rotation = new Vector3(0, yaw, 0);
            Node3D camera = GetNode<Node3D>("CameraPivot");
            camera.Rotation = new Vector3(pitch, 0, 0);
        }
    }

    private void Climbing()
{
    if (wallCheck == null || stillOnWallCheck == null)
        return; // Safety check: exit if RayCasts are missing

    if (stillOnWallCheck.IsColliding())
    {
            if (wallCheck.IsColliding())
            {
                isClimbing = true;
            }
            else
            {
                vel.Y = climbingJump;
                isClimbing = false;

        }
    }
    else
    {
        isClimbing = false;
    }

    if (isClimbing)
    {
        dullGravity = 0;
        Speed = climbingSpeed;

        var rot = -(Mathf.Atan2(wallCheck.GetCollisionNormal().Z, wallCheck.GetCollisionNormal().X) - Mathf.Pi / 2);
        var fInput = Input.GetActionStrength("move_forward") - Input.GetActionStrength("move_back");
        var hInput = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");

        Vector3 wallMoveDir = new Vector3(hInput, fInput, 0).Rotated(Vector3.Up, rot);
        vel = wallMoveDir * climbingSpeed;
    }
    else
    {
        Speed = runSpeed;
        dullGravity = Gravity;
    }
}

}
