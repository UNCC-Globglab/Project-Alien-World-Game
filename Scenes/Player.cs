using Godot;

public partial class Player : CharacterBody2D
{
	// Some of these numbers are big because F = ma
	private float MovementForce = 70000;
	private float Gravity = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");
	private float JumpVelocity = -275;
	private float DragCoefficient = .0043f;
	private float Mass = 70;
	private float FrictionCoefficient = 1.2f;
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
			X = CalculateHorizontalForce(inputVector),
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

	private float CalculateHorizontalForce(Vector2 inputVector)
	{
		var inputMovementForce = inputVector.X * MovementForce;

		float dampingForce = 0f;
		if (Mathf.Abs(Velocity.X) > TargetMaxMovementSpeed && Mathf.Sign(inputMovementForce) == Mathf.Sign(Velocity.X))
		{
			dampingForce = -inputMovementForce;
		}
		return inputMovementForce + dampingForce + CalculateFriction(inputVector.X);
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
		if (Input.IsActionPressed("up") && IsOnFloor())
		{
			return JumpVelocity;
		}

		var acceleration = force / Mass;
		return Velocity.Y + (acceleration * (float)delta);
	}

	private float CalculateFriction(float inputDirectionX)
	{
		float friction;
		if (!IsOnFloor())
		{
			friction = CalculateDrag(Velocity.X);
		}
		else if (Mathf.Sign(inputDirectionX) == Mathf.Sign(Velocity.X))
		{
			friction = 0;
		}
		else
		{
			float normalForce = Mass * Gravity;
			friction = -Mathf.Sign(Velocity.X) * FrictionCoefficient * normalForce;
		}

		return friction;
	}

	private static Vector2 RemoveFloatingErrors(Vector2 vector)
	{
		vector.X = RemoveFloatingErrors(vector.X);
		vector.Y = RemoveFloatingErrors(vector.Y);
		return vector;
	}

	private static float RemoveFloatingErrors(float number)
	{
		if (Mathf.Abs(number) < 10f) return 0f;
		return number;
	}
}
