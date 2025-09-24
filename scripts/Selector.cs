using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


// Aliases to avoid System.Numerics conflicts

using Vector2 = Godot.Vector2;
using Vector2I = Godot.Vector2I;

public partial class Selector : Node2D
{
    [Export] public TileMapLayer WallsLayer;
    [Export] public Node GameManagerNode;
    [Export] public PackedScene CannonScene;
    [Export] public Node2D CannonsLayer;

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

        if (WallsLayer == null)
        {
            GD.PrintErr("Selector: WallsLayer not assigned in Inspector!");
        }
    }

    private void OnPhaseChanged(int newPhase, float difficulty)
    {
        _currentPhase = (GameManager.Phase)newPhase;
        GD.Print($"DEBUG: Selector received phase change -> {_currentPhase}");

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
        var shape = TetrominoShapes[(int)(GD.Randi() % (uint)TetrominoShapes.Length)];
        _currentPiece = new List<Vector2I>(shape);
    }

    private void UpdateAsTetrominoBuilder()
    {
        if (WallsLayer == null) return;

        Vector2 mouseGlobal = GetGlobalMousePosition();
        Vector2 mouseLocal = WallsLayer.ToLocal(mouseGlobal);
        Vector2I gridOrigin = WallsLayer.LocalToMap(mouseLocal);

        var absCells = new List<Vector2I>();
        bool canPlace = true;

        foreach (var offset in _currentPiece)
        {
            Vector2I cell = gridOrigin + offset;
            absCells.Add(cell);

            if (WallsLayer.GetCellSourceId(cell) != -1) // occupied
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
                WallsLayer.SetCell(cell, 0, new Vector2I(0, 0));
            }
            NewPiece();
        }
    }
    private bool IsCannonOverlap(List<Vector2I> cannonCells)
    {
        var spaceState = GetWorld2D().DirectSpaceState;

        // Find footprint center
        Vector2 worldPos = Vector2.Zero;
        foreach (var cell in cannonCells)
            worldPos += WallsLayer.MapToLocal(cell);
        worldPos /= cannonCells.Count;

        // Footprint size (2x2 tiles)
        Vector2I tileSize = WallsLayer.TileSet.TileSize;
        Vector2 footprintSize = (Vector2)tileSize * 2;

        // Create rectangle query
        var rectShape = new RectangleShape2D { Size = footprintSize };

        var query = new PhysicsShapeQueryParameters2D
        {
            Shape = rectShape,
            Transform = new Transform2D(0, worldPos),
            CollideWithAreas = true,
            CollideWithBodies = true
        };

        // Query physics space
        var results = spaceState.IntersectShape(query, 8);

        foreach (var hit in results)
        {
            var collider = hit["collider"];
            if (collider.Obj is Node node && node.IsInGroup("cannon"))
            {
                return true; // overlap detected
            }
        }

        return false;
    }
    private void UpdateAsCannonBuilder()
    {
        if (WallsLayer == null || CannonScene == null || CannonsLayer == null)
            return;

        Vector2 mouseGlobal = GetGlobalMousePosition();
        Vector2 mouseLocal = WallsLayer.ToLocal(mouseGlobal);
        Vector2I gridOrigin = WallsLayer.LocalToMap(mouseLocal);

        // Define a 2x2 area
        var cannonCells = new List<Vector2I>
        {
            gridOrigin,
            gridOrigin + new Vector2I(1, 0),
            gridOrigin + new Vector2I(0, 1),
            gridOrigin + new Vector2I(1, 1)
        };

        bool canPlace = true;

        // 1. Check walls
        foreach (var cell in cannonCells)
        {
            if (WallsLayer.GetCellSourceId(cell) != -1)
            {
                canPlace = false;
                break;
            }
        }

        // 2. Check cannons (full footprint)
        if (canPlace && IsCannonOverlap(cannonCells))
        {
            canPlace = false;
        }

        // 3. Update preview
        _previewCells = cannonCells;
        _previewColor = canPlace ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        QueueRedraw();

        // 4. Place cannon if valid
        if (Input.IsActionJustPressed("mouse_left") && canPlace)
        {
            // Center of footprint
            Vector2 worldPos = Vector2.Zero;
            foreach (var cell in _previewCells)
                worldPos += WallsLayer.MapToLocal(cell);
            worldPos /= _previewCells.Count;

            var cannon = CannonScene.Instantiate<Cannon>();
            cannon.Position = worldPos;
            CannonsLayer.AddChild(cannon);

            GD.Print($"Placed cannon at {worldPos}");
        }
    }
    private void UpdateAsCrosshair()
    {
        GlobalPosition = GetGlobalMousePosition();
        QueueRedraw();

        if (Input.IsActionJustPressed("mouse_left"))
        {
            GD.Print($"Firing cannons at {GlobalPosition}");
        }
    }
    public override void _Draw()
    {
        if (WallsLayer == null) return;

        Vector2I tileSize = WallsLayer.TileSet.TileSize;

        if (_currentPhase == GameManager.Phase.BUILD || _currentPhase == GameManager.Phase.REPAIR)
        {
            if (_previewCells.Count > 0)
            {
                Vector2 cellHalf = (Vector2)tileSize / 2f;
                foreach (Vector2I cell in _previewCells)
                {
                    Vector2 cellCenter = (Vector2)WallsLayer.MapToLocal(cell);
                    DrawRect(new Rect2(cellCenter - cellHalf, (Vector2)tileSize), _previewColor, true);
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
        else if (_currentPhase == GameManager.Phase.CANNONS)
        {
            if (_previewCells.Count > 0)
            {
                Vector2 cellHalf = (Vector2)tileSize / 2f; // reuse existing tileSize
                foreach (Vector2I cell in _previewCells)
                {
                    Vector2 cellCenter = (Vector2)WallsLayer.MapToLocal(cell);
                    DrawRect(new Rect2(cellCenter - cellHalf, (Vector2)tileSize), _previewColor, true);
                }
            }
        }
    }
}
