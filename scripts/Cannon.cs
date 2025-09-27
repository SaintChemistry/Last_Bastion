using Godot;
using System;

public partial class Cannon : StaticBody2D
{
    [Export] public PackedScene ProjectileScene;
    [Export] public int MaxHealth = 100;
    [Export] public float FireCooldown = 0.5f;
    [Export] public NodePath MuzzlePath; // drag the Muzzle node here in inspector

    private int _currentHealth;
    private double _lastFireTime = -999;
    private Marker2D _muzzle;

    public override void _Ready()
    {
        _currentHealth = MaxHealth;
        AddToGroup("cannon");

        // cache the muzzle
        _muzzle = GetNode<Marker2D>("Muzzle"); // always find child directly

        // Debug Start
        DebugCollision.PrintCollision(this);
    }

    public override void _Process(double delta)
    {
        // Always face the cursor
        Vector2 dir = GetGlobalMousePosition() - GlobalPosition;
        Rotation = dir.Angle();
    }

    public void Fire(Vector2 targetPosition)
    {
        if (ProjectileScene == null || _muzzle == null)
            return;

        var projectile = ProjectileScene.Instantiate<Projectile>();
        GetParent().AddChild(projectile);

        // Spawn exactly at the muzzle’s position
        projectile.GlobalPosition = _muzzle.GlobalPosition;

        // Forward direction = muzzle’s local X axis
        Vector2 dir = _muzzle.GlobalTransform.X.Normalized();

        projectile.Velocity = dir * 400f;
        projectile.EnableCollisionDelayed(0.1f);

        // Debug Start
        GD.Print($"Cannon at {GlobalPosition} fired projectile at {_muzzle.GlobalPosition} with dir {dir}");
    }

    public void TakeDamage(int dmg)
    {
        _currentHealth -= dmg;
        GD.Print($"Cannon at {GlobalPosition} took {dmg} damage → HP = {_currentHealth}");

        if (_currentHealth <= 0)
        {
            GD.Print($"Cannon at {GlobalPosition} destroyed!");
            QueueFree();
        }
    }
}
