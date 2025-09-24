using Godot;

public partial class Projectile : Node2D
{
    [Export] public float Speed = 300f;
    public Vector2 Velocity { get; set; } = Vector2.Zero;

    public override void _Process(double delta)
    {
        Position += Velocity * (float)delta;

        // Auto-delete when leaving the screen
        if (!GetViewportRect().HasPoint(GlobalPosition))
            QueueFree();
    }

    private void _OnArea2DBodyEntered(Node body)
    {
        // TODO: Apply damage/effects here
        QueueFree();
    }

}