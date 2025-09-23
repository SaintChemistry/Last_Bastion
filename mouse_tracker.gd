extends Node2D

@export var scroll_speed: float = 400.0
@export var edge_size: int = 20
@export var smooth_factor: float = 8.0
@export var use_edge_scroll: bool = false

var window_rect: Rect2i

func _ready() -> void:
    window_rect = Rect2i(Vector2i.ZERO, get_window().size)

func _process(delta: float) -> void:
    var mouse_pos = get_viewport().get_mouse_position()

    if use_edge_scroll:
        var move_vec = Vector2.ZERO

        if mouse_pos.x <= edge_size:
            move_vec.x -= 1
        elif mouse_pos.x >= window_rect.size.x - edge_size:
            move_vec.x += 1

        if mouse_pos.y <= edge_size:
            move_vec.y -= 1
        elif mouse_pos.y >= window_rect.size.y - edge_size:
            move_vec.y += 1

        if move_vec != Vector2.ZERO:
            global_position += move_vec.normalized() * scroll_speed * delta
    else:
        if window_rect.has_point(mouse_pos):
            var target_pos = get_global_mouse_position()
            global_position = global_position.lerp(target_pos, delta * smooth_factor)

func _notification(what):
    if what == NOTIFICATION_WM_SIZE_CHANGED:
        window_rect = Rect2i(Vector2i.ZERO, get_window().size)
