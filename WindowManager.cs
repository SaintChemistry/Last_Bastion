using Godot;
using System;

public partial class WindowManager : Node
{
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("toggle_fullscreen"))
        {
            var currentMode = DisplayServer.WindowGetMode();

            if (currentMode == DisplayServer.WindowMode.Windowed)
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            }
            else
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                DisplayServer.WindowSetSize(new Vector2I(1920, 1080));
            }
        }
        // Quit Game (ESC)
        if (@event.IsActionPressed("quit_game"))
        {
            GetTree().Quit();
        }
    }

}
