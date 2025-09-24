using Godot;
using System;

public partial class Cannon : StaticBody2D
{
    [Export] public PackedScene ProjectileScene;
    [Export] public int MaxHealth = 100;
    [Export] public float FireCooldown = 0.5f;

    private int _currentHealth;
    private double _lastFireTime = -999;

    public override void _Ready()
    {
        _currentHealth = MaxHealth;
        AddToGroup("cannon");
    }

    public override void _Process(double delta)
    {
        // Always face the cursor
        Vector2 dir = GetGlobalMousePosition() - GlobalPosition;
        Rotation = dir.Angle();
    }

    public void Fire(Vector2 targetPosition)
    {
        if (ProjectileScene == null)
            return;

        // Cooldown check
        double now = Time.GetTicksMsec() / 1000.0;
        if (now - _lastFireTime < FireCooldown)
            return;

        _lastFireTime = now;

        // Spawn projectile
        var projectile = ProjectileScene.Instantiate<Node2D>();
        GetParent().AddChild(projectile);
        projectile.GlobalPosition = GlobalPosition;

        // Give projectile a velocity (projectile must have "velocity" property or script)
        Vector2 dir = (targetPosition - GlobalPosition).Normalized();
        projectile.Set("velocity", dir * 400f);
    }

    public void TakeDamage(int dmg)
    {
        _currentHealth -= dmg;
        GD.Print($"Cannon at {GlobalPosition} took {dmg} damage → HP = {_currentHealth}");

        if (_currentHealth <= 0)
        {
            GD.Print($"Cannon at {GlobalPosition} destroyed!");
            QueueFree(); // remove from scene
        }
    }
}
