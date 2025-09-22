extends Node2D

@export var cell_size: int = 32

func _ready() -> void:
    global_position = snap_to_block(global_position)

func snap_to_block(pos: Vector2) -> Vector2:
    # Convert position into tile coordinates
    var tile_x = floor(pos.x / cell_size)
    var tile_y = floor(pos.y /cell_size)
    
    # snap to the *top-left* tile of a 2x2 block
    var block_x = floor(tile_x / 2) * 2
    var block_y = floor(tile_y / 2) * 2
    
    # Convert back into world position (center of the 2x2 block)
    var snapped_x = (block_x * cell_size) + cell_size
    var snapped_y = (block_y * cell_size) + cell_size
    
    return Vector2(snapped_x, snapped_y)
