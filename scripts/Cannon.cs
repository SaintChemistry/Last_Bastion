using Godot;
using System;

public partial class Cannon : Node2D
{
    [Export] public PackedScene ProjectileScene;

    public override void _Ready()
    {
        AddToGroup("cannon");
    }
    public void Fire(Vector2 targetPosition)
    {
        if (ProjectileScene == null)
        {
            GD.PushWarning("ProjectileScene not assigned!");
            return;
        }

        var projectile = ProjectileScene.Instantiate<Projectile>();
        GetParent().AddChild(projectile);
        projectile.GlobalPosition = GlobalPosition;

        var dir = (targetPosition - GlobalPosition).Normalized();
        projectile.Velocity = dir * 300f;

        GD.Print($"Projectile fired from {GlobalPosition} toward {targetPosition}");
    }
}
