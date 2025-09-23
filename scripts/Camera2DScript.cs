using Godot;
using System;

public partial class Camera2DScript : Camera2D
{
    [Export] public Node2D Target;

    [Export] public float SmoothingSpeed = 2.5f;

    public override void _Process(double delta)
    {
        if (Target != null)
        {
            Position = Position.Lerp(Target.Position, (float)delta * SmoothingSpeed);
        }
    }

}
