extends Sprite2D

@export var walls_layer: TileMapLayer
@export var game_manager: Node  # drag GameManager here in Inspector

var current_phase

func _ready() -> void:
    if game_manager:
        # Connect to GameManager’s signal
        game_manager.connect("phase_changed", Callable(self, "_on_phase_changed"))
        current_phase = game_manager.current_phase

func _on_phase_changed(new_phase) -> void:
    current_phase = new_phase

    match new_phase:
        game_manager.Phase.BUILD, game_manager.Phase.REPAIR:
            texture = preload("res://assets/selector.png")
        game_manager.Phase.BATTLE:
            texture = preload("res://assets/crosshair.png")

func _process(_delta: float) -> void:
    match current_phase:
        game_manager.Phase.BUILD, game_manager.Phase.REPAIR:
            update_as_grid_selector()
        game_manager.Phase.BATTLE:
            update_as_crosshair()

func update_as_grid_selector() -> void:
    var mouse_global = get_global_mouse_position()
    var mouse_local = walls_layer.to_local(mouse_global)
    var grid_coords: Vector2i = walls_layer.local_to_map(mouse_local)
    var tile_center: Vector2 = walls_layer.map_to_local(grid_coords)
    global_position = walls_layer.to_global(tile_center)

    var occupied = walls_layer.get_cell_tile_data(grid_coords) != null
    
 #  Check cannons (2×2 footprint)
    for cannon in get_tree().get_nodes_in_group("cannons"):
        # Cannon origin is centered, so shift back to its top-left tile
        var cannon_top_left = cannon.global_position - Vector2(32, 32)
        var base_tile: Vector2i = walls_layer.local_to_map(walls_layer.to_local(cannon_top_left))

        var covered_tiles = [
            base_tile,
            base_tile + Vector2i(1, 0),
            base_tile + Vector2i(0, 1),
            base_tile + Vector2i(1, 1)
        ]

        if grid_coords in covered_tiles:
            occupied = true
            break

    if occupied:
        modulate = Color(1, 0, 0, 0.6)   # red
    else:
        modulate = Color(0, 1, 0, 0.6)   # green

    if Input.is_action_just_pressed("mouse_left") and not occupied:
        walls_layer.set_cell(grid_coords, 0, Vector2i(0,0))

func update_as_crosshair() -> void:
    global_position = get_global_mouse_position()
    modulate = Color(1, 1, 1, 0.8)

    if Input.is_action_just_pressed("mouse_left"):
        print("Firing cannons at ", global_position)
