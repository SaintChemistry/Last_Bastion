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

    // Debug Export test
    [Export] public int DebugValue = 123;


    public Phase CurrentPhase = Phase.BUILD;
    private float _remainingTime = 0.0f;
    private int _lastTimeLeft = -1;
    private float _difficulty = 1.0f;

    public override void _Ready()
    {
        StartPhase(Phase.BUILD, 20f);
    }

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
