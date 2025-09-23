extends Node2D

@export var walls_layer: TileMapLayer
@export var game_manager: Node

var current_phase: int
var current_piece: Array[Vector2i] = []
var preview_cells: Array[Vector2i] = []
var preview_color: Color = Color(0,1,0,0.5)

# Define tetromino shapes (outer Array untyped, inner arrays of Vector2i)
var tetromino_shapes: Array = [
    [Vector2i(0,0)], # Single block
    [Vector2i(0,0), Vector2i(1,0), Vector2i(0,1), Vector2i(1,1)],  # Square
    [Vector2i(0,0), Vector2i(1,0), Vector2i(2,0), Vector2i(3,0)],  # Line
    [Vector2i(0,0), Vector2i(1,0), Vector2i(1,1), Vector2i(2,1)],  # Z-shape
    [Vector2i(0,0), Vector2i(0,1), Vector2i(1,1), Vector2i(2,1)],  # L-shape
    [Vector2i(1,0), Vector2i(0,1), Vector2i(1,1), Vector2i(2,1)]   # T-shape
]

func _ready() -> void:
    if game_manager:
        game_manager.connect("phase_changed", self._on_phase_changed)
        current_phase = game_manager.current_phase

func _on_phase_changed(new_phase, _difficulty) -> void:
    current_phase = new_phase

    match new_phase:
        game_manager.Phase.BUILD, game_manager.Phase.REPAIR:
            new_piece()
            visible = true
        game_manager.Phase.CANNONS, game_manager.Phase.BATTLE:
            preview_cells.clear()
            visible = true
        _:
            visible = false

func _process(_delta: float) -> void:
    match current_phase:
        game_manager.Phase.BUILD, game_manager.Phase.REPAIR:
            update_as_tetromino_builder()
        game_manager.Phase.CANNONS:
            update_as_cannon_builder()
        game_manager.Phase.BATTLE:
            update_as_crosshair()

# --- BUILD / REPAIR: Tetromino placement ---
func new_piece():
    var shape: Array = tetromino_shapes.pick_random()
    current_piece.clear()
    for offset in shape:
        current_piece.append(offset as Vector2i)

func update_as_tetromino_builder() -> void:
    var mouse_global = get_global_mouse_position()
    var mouse_local = walls_layer.to_local(mouse_global)
    var grid_origin: Vector2i = walls_layer.local_to_map(mouse_local)

    var abs_cells: Array[Vector2i] = []
    var can_place = true

    for offset in current_piece:
        var cell = grid_origin + offset
        abs_cells.append(cell)

        # Check walls
        if walls_layer.get_cell_tile_data(cell) != null:
            can_place = false

        # Check cannons (2×2 footprint)
        for cannon in get_tree().get_nodes_in_group("cannons"):
            var cannon_top_left = cannon.global_position - Vector2(32, 32)
            var base_tile: Vector2i = walls_layer.local_to_map(walls_layer.to_local(cannon_top_left))
            var covered_tiles = [
                base_tile,
                base_tile + Vector2i(1, 0),
                base_tile + Vector2i(0, 1),
                base_tile + Vector2i(1, 1)
            ]
            if cell in covered_tiles:
                can_place = false
                break

    preview_cells = abs_cells
    preview_color = Color(0,1,0,0.5) if can_place else Color(1,0,0,0.5)
    queue_redraw()

    if Input.is_action_just_pressed("mouse_left") and can_place:
        for cell in abs_cells:
            walls_layer.set_cell(cell, 0, Vector2i(0,0))
        new_piece()


func _draw() -> void:
    var tile_size: Vector2i = walls_layer.tile_set.tile_size

    # Tetromino preview (BUILD/REPAIR)
    if current_phase == game_manager.Phase.BUILD or current_phase == game_manager.Phase.REPAIR:
        if not preview_cells.is_empty():
            var cell_half = Vector2(tile_size) / 2.0
            for cell in preview_cells:
                var cell_center: Vector2 = walls_layer.map_to_local(cell)
                draw_rect(Rect2(cell_center - cell_half, tile_size), preview_color, true)

    # Crosshair (BATTLE only)
    elif current_phase == game_manager.Phase.BATTLE:
        var size = 16
        var color = Color(1, 1, 1, 0.8)
        draw_line(Vector2(-size, 0), Vector2(size, 0), color, 2.0)
        draw_line(Vector2(0, -size), Vector2(0, size), color, 2.0)

# --- CANNONS (placeholder) ---
func update_as_cannon_builder() -> void:
    preview_cells.clear()
    queue_redraw()
    if Input.is_action_just_pressed("mouse_left"):
        print("Placing cannon (stub)")

# --- BATTLE ---
func update_as_crosshair() -> void:
    global_position = get_global_mouse_position()
    queue_redraw()

    if Input.is_action_just_pressed("mouse_left"):
        print("Firing cannons at ", global_position)
