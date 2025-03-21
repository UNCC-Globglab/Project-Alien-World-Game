using Godot;
using System;

public partial class Player : CharacterBody2D
{
    private float speed = 130;
    private float gravity = (float) ProjectSettings.GetSetting("physics/2d/default_gravity");
    private float jumpVelocity = -300f;

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = new()
        {
            X = Input.GetAxis("left", "right"),
            Y = Velocity.Y,
        };

        if (Input.IsActionPressed("up") && IsOnFloor())
        {
            velocity.Y += jumpVelocity;
        }

        velocity.X *= speed;
        velocity.Y += gravity * (float) delta;

        Velocity = velocity;

        MoveAndSlide();
    }
}
