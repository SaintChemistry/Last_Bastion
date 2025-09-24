using Godot;
using System;

public partial class Cannon : Node2D
{
    [Export] public PackedScene ProjectileScene;

    public override void _Ready()
    {
        AddToGroup("cannon");
    }
    public void Fire(Vector2 targetpPosition)
    {
        if (ProjectileScene == null)
            return;

        var projectile = ProjectileScene.Instantiate<Projectile>();
        GetParent().AddChild(projectile);
        projectile.GlobalPosition = GlobalPosition;

        // If your projectile has a velocity or direction method, set it here
        var dir = (targetpPosition - GlobalPosition).Normalized();
        projectile.Velocity = dir * 300f;
    }
}
