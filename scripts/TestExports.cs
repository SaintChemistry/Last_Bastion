using Godot;
using System;

public partial class TestExports : Node
{
    [Export] public int TestValue = 42;
    [Export] public Label SomeLabel;
}
