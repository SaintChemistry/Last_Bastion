extends Node

enum Phase { BUILD, REPAIR, BATTLE }

@onready var phase_timer: Timer = $PhaseTimer
@onready var phase_label: Label = $CanvasLayer/PhaseLabel
@onready var beep_player: AudioStreamPlayer = $BeepPlayer

var current_phase: Phase = Phase.BUILD
var remaining_time: float = 0.0
var last_time_left: int = -1   # for beep tracking

func _ready() -> void:
    start_phase(Phase.BUILD, 20)

func start_phase(phase: Phase, duration: float) -> void:
    current_phase = phase
    remaining_time = duration
    last_time_left = int(duration)

    match phase:
        Phase.BUILD:
            phase_label.text = "BUILD PHASE: %d" % remaining_time
        Phase.REPAIR:
            phase_label.text = "REPAIR PHASE: %d" % remaining_time
        Phase.BATTLE:
            phase_label.text = "BATTLE PHASE"

    if phase == Phase.BATTLE:
        phase_timer.stop()
    else:
        phase_timer.wait_time = duration
        phase_timer.start()

func _process(delta: float) -> void:
    if phase_timer.is_stopped():
        return

    remaining_time = max(0, remaining_time - delta)
    var time_left = int(remaining_time)

    if current_phase == Phase.BUILD:
        phase_label.text = "BUILD PHASE: %d" % time_left
    elif current_phase == Phase.REPAIR:
        phase_label.text = "REPAIR PHASE: %d" % time_left

    # Play beep on integer second change in last 5s
    if (current_phase == Phase.BUILD or current_phase == Phase.REPAIR) and time_left <= 5:
        if time_left != last_time_left:
            beep_player.play()
            last_time_left = time_left

        # Flash red/white
        var flash = int(Time.get_ticks_msec() / 500) % 2 == 0
        if flash:
            phase_label.add_theme_color_override("font_color", Color(1, 0, 0)) # red
        else:
            phase_label.add_theme_color_override("font_color", Color(1, 1, 1)) # white
    else:
        # Reset color + last time tracker
        phase_label.add_theme_color_override("font_color", Color(1, 1, 1))
        last_time_left = time_left

func _on_phase_timer_timeout() -> void:
    match current_phase:
        Phase.BUILD:
            print("Switching to BATTLE phase")
            start_phase(Phase.BATTLE, 0)
        Phase.BATTLE:
            print("Switching to REPAIR phase")
            start_phase(Phase.REPAIR, 20)
        Phase.REPAIR:
            print("Switching to BATTLE phase")
            start_phase(Phase.BATTLE, 0)
