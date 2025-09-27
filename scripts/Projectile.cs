using Godot;
using System;

public partial class Projectile : Area2D
{
    [Export] public int Damage = 25;
    [Export] public float Speed = 400f;

    public Vector2 Velocity = Vector2.Zero;

    private CollisionShape2D _collisionShape;

    public override void _Ready()
    {
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        // Connect collision signals
        BodyEntered += OnBodyEntered;
    }

    public override void _Process(double delta)
    {
        GlobalPosition += Velocity * (float)delta;

        // Use central bounds from GameManager
        if (!GameManager.PlayfieldBounds.HasPoint(GlobalPosition))
            QueueFree();
    }

    private void OnBodyEntered(Node body)
    {
        if (body.IsInGroup("enemy"))
        {
            GD.Print($"Projectile hit enemy: {body.Name}");
            if (body.HasMethod("TakeDamage"))
                body.Call("TakeDamage", Damage);

            QueueFree();
        }
    }

    public async void EnableCollisionDelayed(float delay = 0.1f)
    {
        if (_collisionShape != null)
        {
            _collisionShape.Disabled = true;
            await ToSignal(GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);
            if (IsInstanceValid(_collisionShape))
                _collisionShape.Disabled = false;
        }
    }
}
