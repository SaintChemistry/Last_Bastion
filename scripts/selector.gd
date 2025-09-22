extends Sprite2D

@export var walls_layer: TileMapLayer

func _process(_delta: float) -> void:
    if walls_layer == null:
        modulate = Color(1, 1, 0, 0.6) # yellow = debug if no walls layer
        return

    # Mouse position (global)
    var mouse_global = get_global_mouse_position()
    var mouse_local = walls_layer.to_local(mouse_global)
    var grid_coords: Vector2i = walls_layer.local_to_map(mouse_local)
    var tile_center: Vector2 = walls_layer.map_to_local(grid_coords)
    global_position = walls_layer.to_global(tile_center)

    # Check walls
    var wall_tile = walls_layer.get_cell_tile_data(grid_coords)
    var occupied = wall_tile != null

    # Check cannons (2x2 footprint)
    for cannon in get_tree().get_nodes_in_group("cannons"):
        # Cannon’s origin is at center → shift back to top-left
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
            break   # safe here, because we are still inside the for-loop

    # Color feedback
    if occupied:
        modulate = Color(1, 0, 0, 0.6)   # red
    else:
        modulate = Color(0, 1, 0, 0.6)   # green
