using Godot;
using System;

public partial class GameManager : Node
{
    public enum Phase { BUILD, CANNONS, REPAIR, BATTLE }

    [Signal]
    public delegate void PhaseChangedEventHandler(int newPhase, float difficulty);

    [Export] public Timer PhaseTimer;
    [Export] public Label PhaseLabel;
    [Export] public AudioStreamPlayer BeepPlayer;

    // === NEW: Playfield bounds ===
    public static Rect2 PlayfieldBounds { get; private set; } = new Rect2(-1000, -1000, 2000, 2000);

    public Phase CurrentPhase = Phase.BUILD;
    private float _remainingTime = 0.0f;
    private int _lastTimeLeft = -1;
    private float _difficulty = 1.0f;

    public override void _Ready()
    {
        if (PhaseTimer != null)
        {
            PhaseTimer.Timeout += _OnPhaseTimerTimeout;
        }

        // Calculate playfield once at start (use "Walls" TileMapLayer)
        var wallsLayer = GetTree().Root.GetNodeOrNull<TileMapLayer>("Main/Walls");
        if (wallsLayer != null)
        {
            PlayfieldBounds = CalculateTilemapBounds(wallsLayer);
            GD.Print($"[GameManager] Playfield bounds initialized = {PlayfieldBounds}");
        }

        StartPhase(Phase.BUILD, 20f);
    }

    // === NEW: helper to recalc when regenerating terrain ===
    public void UpdatePlayfieldBounds(TileMapLayer layer)
    {
        PlayfieldBounds = CalculateTilemapBounds(layer);
        GD.Print($"[GameManager] Playfield bounds updated = {PlayfieldBounds}");
    }

    private Rect2 CalculateTilemapBounds(TileMapLayer layer)
    {
        Rect2I used = layer.GetUsedRect();

        Vector2 worldTopLeft = layer.MapToLocal(used.Position);
        Vector2 worldBottomRight = layer.MapToLocal(used.End);

        Vector2 worldSize = worldBottomRight - worldTopLeft;
        Vector2 half = worldSize / 2f;

        // Center bounds on (0,0)
        return new Rect2(-half, worldSize);
    }

    // === Existing phase code unchanged ===
    private void StartPhase(Phase phase, float duration)
    {
        CurrentPhase = phase;
        _remainingTime = duration;
        _lastTimeLeft = (int)duration;

        EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase, _difficulty);

        switch (phase)
        {
            case Phase.BUILD:
                PhaseLabel.Text = $"BUILD PHASE: {duration}";
                break;
            case Phase.CANNONS:
                PhaseLabel.Text = $"CANNONS PHASE: {duration}";
                break;
            case Phase.REPAIR:
                PhaseLabel.Text = $"REPAIR PHASE: {duration}";
                break;
            case Phase.BATTLE:
                PhaseLabel.Text = $"BATTLE PHASE (x{_difficulty:F2})";
                break;
        }

        if (phase == Phase.BATTLE)
        {
            PhaseTimer.Stop();
        }
        else
        {
            PhaseTimer.WaitTime = duration;
            PhaseTimer.Start();
        }
    }

    public override void _Process(double delta)
    {
        if (PhaseTimer.IsStopped())
            return;

        _remainingTime = Math.Max(0, _remainingTime - (float)delta);
        int timeLeft = (int)_remainingTime;

        switch (CurrentPhase)
        {
            case Phase.BUILD:
                PhaseLabel.Text = $"BUILD PHASE: {timeLeft}";
                break;
            case Phase.CANNONS:
                PhaseLabel.Text = $"CANNONS PHASE: {timeLeft}";
                break;
            case Phase.REPAIR:
                PhaseLabel.Text = $"REPAIR PHASE: {timeLeft}";
                break;
        }

        // Beep + flash for last 5 seconds
        if ((CurrentPhase == Phase.BUILD || CurrentPhase == Phase.CANNONS || CurrentPhase == Phase.REPAIR) && timeLeft <= 5)
        {
            if (timeLeft != _lastTimeLeft)
            {
                BeepPlayer.Play();
                _lastTimeLeft = timeLeft;
            }

            bool flash = (Time.GetTicksMsec() / 500) % 2 == 0;
            PhaseLabel.AddThemeColorOverride("font_color", flash ? new Color(1, 0, 0) : new Color(1, 1, 1));
        }
        else
        {
            PhaseLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));
            _lastTimeLeft = timeLeft;
        }
    }

    private void _OnPhaseTimerTimeout()
    {
        switch (CurrentPhase)
        {
            case Phase.BUILD:
                GD.Print("Switching to CANNONS phase");
                StartPhase(Phase.CANNONS, 10f);
                break;
            case Phase.CANNONS:
                GD.Print("Switching to BATTLE phase");
                StartPhase(Phase.BATTLE, 0f);
                break;
            case Phase.BATTLE:
                GD.Print("Switching to REPAIR phase");
                StartPhase(Phase.REPAIR, 20f);
                break;
            case Phase.REPAIR:
                _difficulty += 0.05f;
                GD.Print($"Repair finished → increasing difficulty to {_difficulty:F2}");
                StartPhase(Phase.BUILD, 20f);
                break;
        }
    }
}
