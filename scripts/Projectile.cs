using Godot;
using System;

public partial class Projectile : Area2D
{
    [Export] public int Damage = 25;
    [Export] public float Speed = 400f;

    public Vector2 Velocity;

    public override void _Process(double delta)
    {
        GlobalPosition += Velocity * (float)delta;

        // Remove if outside viewport bounds
        if (!GetViewportRect().HasPoint(GlobalPosition))
            QueueFree();
    }

    private void _OnBodyEntered(Node body)
    {
        if (body is Cannon cannon)
        {
            cannon.TakeDamage(Damage);
            QueueFree();
        }
    }

}