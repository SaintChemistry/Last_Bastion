using Godot;
using System;
using System.Collections.Generic;
using System.Numerics;
using Vector2 = Godot.Vector2;
using Vector2i = Godot.Vector2I;

public partial class Selector : Node2D
{
    [Export] public TileMapLayer WallsLayer;
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
        // mouse positions are floats (Vector2)
        Vector2 mouseGlobal = GetGlobalMousePosition();
        Vector2 mouseLocal = WallsLayer.ToLocal(mouseGlobal);

        // grid origin is ints (Vector2I)
        Vector2I gridOrigin = WallsLayer.LocalToMap(mouseLocal);

        var absCells = new List<Vector2I>();
        bool canPlace = true;

        foreach (var offset in _currentPiece)
        {
            Vector2I cell = gridOrigin + offset;
            absCells.Add(cell);

            // TileMapLayer API → just takes Vector2I now
            if (WallsLayer.GetCellSourceId(cell) != -1)
                canPlace = false;
        }

        _previewCells = absCells;
        _previewColor = canPlace
            ? new Color(0, 1, 0, 0.5f)
            : new Color(1, 0, 0, 0.5f);

        QueueRedraw();

        if (Input.IsActionJustPressed("mouse_left") && canPlace)
        {
            foreach (var cell in absCells)
            {
                // new SetCell signature: (Vector2I coords, int sourceId, Vector2I? atlasCoords = null, int alternativeTile = 0)
                WallsLayer.SetCell(cell, 0, new Vector2I(0, 0));
            }
            NewPiece();
        }
    }

    public override void _Draw()
    {
        var tileSize = WallsLayer.TileSet.TileSize; // Vector2I

        if (_currentPhase == GameManager.Phase.BUILD || _currentPhase == GameManager.Phase.REPAIR)
        {
            if (_previewCells.Count > 0)
            {
                var cellHalf = (Vector2)tileSize / 2f; // cast to Vector2 for drawing
                foreach (var cell in _previewCells)
                {
                    var cellCenter = (Vector2)WallsLayer.MapToLocal(cell);
                    DrawRect(new Rect2(cellCenter - cellHalf, (Vector2)tileSize), _previewColor, true);
                }
            }
        }
        else if (_currentPhase == GameManager.Phase.BATTLE)
        {
            int size = 16;
            var color = new Color(1, 1, 1, 0.8f);

            // Force Godot.Vector2 to avoid conflict
            DrawLine(new Godot.Vector2(-size, 0), new Godot.Vector2(size, 0), color, 2.0f);
            DrawLine(new Godot.Vector2(0, -size), new Godot.Vector2(0, size), color, 2.0f);
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
        // GlobalPosition is a Godot.Vector2
        GlobalPosition = GetGlobalMousePosition();

        QueueRedraw();

        if (Input.IsActionJustPressed("mouse_left"))
        {
            GD.Print($"Firing cannons at {GlobalPosition}");
        }
    }
}