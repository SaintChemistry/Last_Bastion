using Godot;

public partial class Camera2DScript : Camera2D
{
    [Export(PropertyHint.Range, "0.01,0.25,0.01")]
    public float MarginPercent = 0.05f; // 5% of screen size

    [Export]
    public float CameraSpeed = 400f; // pixels per second

    public override void _Process(double delta)
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;

        float marginX = viewportSize.X * MarginPercent;
        float marginY = viewportSize.Y * MarginPercent;

        Vector2 movement = Vector2.Zero;

        // Horizontal margins
        if (mousePos.X < marginX)
            movement.X = -1;
        else if (mousePos.X > viewportSize.X - marginX)
            movement.X = 1;

        // Vertical margins
        if (mousePos.Y < marginY)
            movement.Y = -1;
        else if (mousePos.Y > viewportSize.Y - marginY)
            movement.Y = 1;

        // Apply movement if needed
        if (movement != Vector2.Zero)
        {
            movement = movement.Normalized();
            Position += movement * CameraSpeed * (float)delta;
        }
    }
}
