using Godot;

public partial class Player : CharacterBody2D
{
    // Some of these numbers are big because F = ma
    private float MovementAcceleration = 1000;
    private float Gravity = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");
    private float JumpVelocity = -300;
    private float DragCoefficient = .0043f;
    private float Mass = 84;
    private float FrictionCoefficient = 2f;
    private float TargetMaxMovementSpeed = 300f;

    public override void _PhysicsProcess(double delta)
    {
        Vector2 inputVector = new()
        {
            X = Input.GetAxis("left", "right"),
            Y = Input.GetAxis("up", "down"),
        };

        Vector2 forces = new()
        {
            X = CalculateHorizontalForce(inputVector, delta),
            Y = CalculateVerticalForce(),
        };

        Vector2 newVelocity = new()
        {
            X = CalculateHorizontalSpeed(forces.X, delta),
            Y = CalculateVerticalSpeed(forces.Y, delta)
        };

        Velocity = RemoveFloatingErrors(newVelocity);

        MoveAndSlide();
    }

    private float CalculateHorizontalForce(Vector2 inputVector, double delta)
    {
        var inputMovementForce = inputVector.X * MovementAcceleration * Mass;

        float dampingForce = 0f;
        if (Mathf.Abs(Velocity.X) > TargetMaxMovementSpeed && Mathf.Sign(inputMovementForce) == Mathf.Sign(Velocity.X))
        {
            dampingForce = -inputMovementForce;
        }
        return inputMovementForce + dampingForce + CalculateFriction(inputVector.X, delta);
    }

    private float CalculateVerticalForce()
    {
        if (IsOnFloor()) return 0;

        var forceDrag = CalculateDrag(Velocity.Y);
        var forceGravity = Mass * Gravity;
        return forceDrag + forceGravity;
    }

    private float CalculateDrag(float velocity)
    {
        return -Mathf.Sign(velocity) * DragCoefficient * float.Pow(velocity, 2);
    }

    private float CalculateHorizontalSpeed(float force, double delta)
    {
        var acceleration = force / Mass;
        return Velocity.X + acceleration * (float)delta;
    }

    private float CalculateVerticalSpeed(float force, double delta)
    {
        if (Input.IsActionJustPressed("up") && IsOnFloor())
        {
            return JumpVelocity;
        }

        var acceleration = force / Mass;
        return Velocity.Y + (acceleration * (float)delta);
    }

    private float CalculateFriction(float inputDirectionX, double delta)
    {
        if (!IsOnFloor())
        {
            return CalculateDrag(Velocity.X);
        }

        // No friction when actively inputting in the direction of movement
        if (inputDirectionX != 0 && Mathf.Sign(inputDirectionX) == Mathf.Sign(Velocity.X))
        {
            return 0;
        }

        // For a natural sliding feel, we want friction to gradually slow the player
        float normalForce = Mass * Gravity;
        float maxFriction = FrictionCoefficient * normalForce;

        // Ensure we don't apply more force than needed to stop in this frame
        // This prevents overshooting (oscillating around zero)
        float stopForce = Mathf.Abs(Velocity.X) * Mass / (float)delta;

        // Use the smaller value to prevent overshooting
        float finalFriction = Mathf.Min(maxFriction, stopForce);

        // Only apply friction if we're actually moving
        if (Mathf.Abs(Velocity.X) < 0.01f)
        {
            return 0;
        }

        return -Mathf.Sign(Velocity.X) * finalFriction;
    }

    private static Vector2 RemoveFloatingErrors(Vector2 vector)
    {
        vector.X = RemoveFloatingErrors(vector.X);
        vector.Y = RemoveFloatingErrors(vector.Y);
        return vector;
    }

    private static float RemoveFloatingErrors(float number)
    {
        if (Mathf.Abs(number) < .1f) return 0f;
        return number;
    }
}
