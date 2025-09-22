extends Camera2D

@export var target: NodePath   # assign the Selector node here in Inspector

func _process(_delta: float) -> void:
    if has_node(target):
        var selector = get_node(target)
        global_position = selector.global_position
