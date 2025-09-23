using Godot;
using System;
using System.Collections.Generic;

public partial class Selector : Node2D
{
    [Export] public TileMap WallsLayer;
    [Export] public Node GameManagerNode;

    private GameManager.Phase _currentPhase;
    private List<Vector2I> _currentPiece = new();
    private List<Vector2I> _previewCells = new();
    private Color _previewColor = new(0, 1, 0, 0.5f);

    private static readonly Vector2I[][] TetrominoShapes = new Vector2I[][]
    {
        new [] { new Vector2I(0,0) }, // Single
        new [] { new Vector2I(0,0), new Vector2I(1,0), new Vector2I(0,1), new Vector2I(1,1) }, // Square
        new [] { new Vector2I(0,0), new Vector2I(1,0), new Vector2I(2,0), new Vector2I(3,0) }, // Line
        new [] { new Vector2I(0,0), new Vector2I(1,0), new Vector2I(1,1), new Vector2I(2,1) }, // Z
        new [] { new Vector2I(0,0), new Vector2I(0,1), new Vector2I(1,1), new Vector2I(2,1) }, // L
        new [] { new Vector2I(1,0), new Vector2I(0,1), new Vector2I(1,1), new Vector2I(2,1) }  // T
    };

    public override void _Ready()
    {
        if (GameManagerNode is GameManager gm)
        {
            gm.PhaseChanged += OnPhaseChanged;
            _currentPhase = gm.CurrentPhase;
        }
    }

    private void OnPhaseChanged(int newPhase, float difficulty)
    {
        _currentPhase = (GameManager.Phase)newPhase;

        switch (_currentPhase)
        {
            case GameManager.Phase.BUILD:
            case GameManager.Phase.REPAIR:
                NewPiece();
                Visible = true;
                break;
            case GameManager.Phase.CANNONS:
            case GameManager.Phase.BATTLE:
                _previewCells.Clear();
                Visible = true;
                break;
            default:
                Visible = false;
                break;
        }
    }

    public override void _Process(double delta)
    {
        switch (_currentPhase)
        {
            case GameManager.Phase.BUILD:
            case GameManager.Phase.REPAIR:
                UpdateAsTetrominoBuilder();
                break;
            case GameManager.Phase.CANNONS:
                UpdateAsCannonBuilder();
                break;
            case GameManager.Phase.BATTLE:
                UpdateAsCrosshair();
                break;
        }
    }

    private void NewPiece()
    {
        var shape = TetrominoShapes[GD.Randi() % TetrominoShapes.Length];
        _currentPiece = new List<Vector2I>(shape);
    }

    private void UpdateAsTetrominoBuilder()
    {
        var mouseGlobal = GetGlobalMousePosition();
        var mouseLocal = WallsLayer.ToLocal(mouseGlobal);
        var gridOrigin = WallsLayer.LocalToMap(mouseLocal);

        var absCells = new List<Vector2I>();
        bool canPlace = true;

        foreach (var offset in _currentPiece)
        {
            var cell = gridOrigin + offset;
            absCells.Add(cell);
            if (WallsLayer.GetCellSourceId(0, cell) != -1) // occupied
                canPlace = false;
        }

        _previewCells = absCells;
        _previewColor = canPlace ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        QueueRedraw();

        if (Input.IsActionJustPressed("mouse_left") && canPlace)
        {
            foreach (var cell in absCells)
                WallsLayer.SetCell(0, cell, 0, new Vector2I(0, 0));
            NewPiece();
        }
    }

    public override void _Draw()
    {
        var tileSize = WallsLayer.TileSet.TileSize;

        if (_currentPhase == GameManager.Phase.BUILD || _currentPhase == GameManager.Phase.REPAIR)
        {
            if (_previewCells.Count > 0)
            {
                var cellHalf = tileSize / 2;
                foreach (var cell in _previewCells)
                {
                    var cellCenter = WallsLayer.MapToLocal(cell);
                    DrawRect(new Rect2(cellCenter - cellHalf, tileSize), _previewColor, true);
                }
            }
        }
        else if (_currentPhase == GameManager.Phase.BATTLE)
        {
            int size = 16;
            var color = new Color(1, 1, 1, 0.8f);
            DrawLine(new Vector2(-size, 0), new Vector2(size, 0), color, 2.0f);
            DrawLine(new Vector2(0, -size), new Vector2(0, size), color, 2.0f);
        }
    }

    private void UpdateAsCannonBuilder()
    {
        _previewCells.Clear();
        QueueRedraw();
        if (Input.IsActionJustPressed("mouse_left"))
            GD.Print("Placing cannon (stub)");
    }

    private void UpdateAsCrosshair()
    {
        GlobalPosition = GetGlobalMousePosition();
        QueueRedraw();

        if (Input.IsActionJustPressed("mouse_left"))
            GD.Print($"Firing cannons at {GlobalPosition}");
    }
}
