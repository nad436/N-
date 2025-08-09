using Godot;
using System;

public partial class CharacterBody3d : CharacterBody3D
{

    [Export] public float Speed { get; set; } = 6f;


    [Export] public float CrouchedSpeed { get; set; } = 2f;
    [Export] public int fallAcceleration { get; set; } = 20;

    [Export] public int jumpImpulse { get; set; } = 20;

    [Export] public int doubleJumpImpulse { get; set; } = 15;

    [Export] public float mouseSensitivity { get; set; } = 3.0f;

    [Export] public float accelerationGround { get; set; } = 10f;
    [Export] public float decelerationGround { get; set; } = 10f;


    [Export] public float accelerationAir { get; set; } = 5f;
    [Export] public float decelerationAir { get; set; } = 2f;
    [Export] public float dashSpeed { get; set; } = 20f;
    [Export] public float dashTime { get; set; } = 0.2f;
    [Export] public float dashCd { get; set; } = 0.5f;

    //Labels
    private Label speedLabel;
    private ColorRect dashIndicator;

    private Vector3 dashDirection = Vector3.Zero;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCdTimer = 0f;
    private Vector3 targetVelocity = Vector3.Zero;

    private bool doubleJumpCheck = true;

    public bool isCrouched;

    public override void _Ready()
    {
        base._Ready();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        speedLabel = GetNode<Label>("CanvasLayer/SpeedLabel");
        dashIndicator = GetNode<ColorRect>("CanvasLayer/ColorRectDash");

    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input.IsActionJustPressed("ESC"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        if (dashCdTimer > 0)
            dashCdTimer -= (float)delta;

        float currentSpeed = new Vector3(Velocity.X, 0, Velocity.Z).Length();
        speedLabel.Text = $"Speed: {Mathf.Round(currentSpeed)}";
        float dashCdIndicator = Mathf.Max(dashCdTimer, 0f);

        if (dashCdIndicator <= 0)
        {
            dashIndicator.Color = new Color(0, 1, 0);
        }
        else
        {
            dashIndicator.Color = new Color(1, 0, 0);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }



    public override void _PhysicsProcess(double delta)
    {
        var inputDir = Vector3.Zero;

        if (Input.IsActionPressed("move_left"))
            inputDir.Z += 1.0f;
        if (Input.IsActionPressed("move_right"))
            inputDir.Z -= 1.0f;
        if (Input.IsActionPressed("move_back"))
            inputDir.X -= 1.0f;
        if (Input.IsActionPressed("move_forward"))
            inputDir.X += 1.0f;

        inputDir = inputDir.Normalized();

        Vector3 forward = -Transform.Basis.Z;
        Vector3 right = Transform.Basis.X;
        Vector3 moveDir = (right * inputDir.X + forward * inputDir.Z).Normalized();

        bool onGround = IsOnFloor();
        float accel = onGround ? accelerationGround : accelerationAir;
        float deccel = onGround ? decelerationGround : decelerationAir;

        Vector3 vel = Velocity;

        if (isDashing)
        {
            dashTimer -= (float)delta;
            Velocity = dashDirection * dashSpeed;

            if (dashTimer <= 0)
            {
                isDashing = false;
                dashCdTimer = dashCd;
            }

            MoveAndSlide();
            return;
        }

        if (Input.IsActionJustPressed("dash") && dashCdTimer <= 0 && moveDir != Vector3.Zero)
        {
            isDashing = true;
            dashTimer = dashTime;
            dashDirection = moveDir;
            Velocity = dashDirection * dashSpeed;
            MoveAndSlide();
            return;
        }

        Vector3 targetVel = moveDir * Speed;

        float speed = Speed;
        if (Input.IsActionPressed("crouch"))
        {
            speed = CrouchedSpeed;
            if (!isCrouched)
            {
                GetNode<AnimationPlayer>("AnimationPlayer").Play("Crouch");
                isCrouched = true;
            }
        }
        else
        {
            if (isCrouched)
            {
                var spaceState = GetWorld3D().DirectSpaceState;
                var result = spaceState.IntersectRay(new PhysicsRayQueryParameters3D() { From = Position, To = new Vector3(Position.X, Position.Y + 2, Position.Z), Exclude = new Godot.Collections.Array<Rid> { GetRid() } });
                if (result.Count <= 0)
                {
                    GetNode<AnimationPlayer>("AnimationPlayer").Play("Uncrouch");
                    isCrouched = false;
                }

            }
        }


        if (moveDir != Vector3.Zero)
        {
            vel.X = Mathf.MoveToward(vel.X, targetVel.X * speed, accel * (float)delta);
            vel.Z = Mathf.MoveToward(vel.Z, targetVel.Z * speed, accel * (float)delta);
        }

        else
        {
            vel.X = Mathf.MoveToward(vel.X, 0, deccel * (float)delta);
            vel.Z = Mathf.MoveToward(vel.Z, 0, deccel * (float)delta);
        }


        if (!IsOnFloor())
            vel.Y -= fallAcceleration * (float)delta;


        if (IsOnFloor())
        {
            doubleJumpCheck = true;

            if (Input.IsActionJustPressed("jump"))
            {
                vel.Y = jumpImpulse;
            }
        }
        else
        {


            if (Input.IsActionJustPressed("jump") && doubleJumpCheck)
            {
                vel.Y = doubleJumpImpulse;
                doubleJumpCheck = false;
            }
        }



        Velocity = vel;
        MoveAndSlide();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventMouseMotion)
        {
            InputEventMouseMotion motion = @event as InputEventMouseMotion;
            Rotation = new Vector3(Rotation.X, Rotation.Y - motion.Relative.X / 1000 * mouseSensitivity, Rotation.Z);
            Camera3D camera = GetNode<Camera3D>("Camera3D");

            camera.Rotation = new Vector3(Mathf.Clamp(camera.Rotation.X - motion.Relative.Y / 1000 * mouseSensitivity, -2, 2), camera.Rotation.Y, camera.Rotation.Z);
        }
    }
    


}
