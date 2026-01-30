extends RigidBody2D
class_name CutableRidgidBody

@export var texture_size := 128.0
@export var polygon_bounding_box := 550.0

@onready var collision_polygon_2d : CollisionPolygon2D = $CollisionPolygon2D
@onready var polygon_2d : Polygon2D = $Polygon2D

func _ready():
	self.polygon_2d.polygon = self.collision_polygon_2d.polygon

func copy_texture_data(other_body: CutableRidgidBody):
	other_body.polygon_2d.texture = self.polygon_2d.texture
	other_body.polygon_2d.scale = self.polygon_2d.scale
	other_body.mass = self.mass
	other_body.freeze = self.freeze
	var scale_factor = self.texture_size / self.polygon_bounding_box
	for vec in other_body.polygon_2d.polygon:
		other_body.polygon_2d.uv.append(scale_factor * vec)
	other_body.texture_size = texture_size
	other_body.polygon_bounding_box = polygon_bounding_box
	other_body.collision_layer = self.collision_layer
	other_body.collision_mask = self.collision_mask

func set_shape(polygon_shape : PackedVector2Array):
	self.collision_polygon_2d.set_deferred("polygon", polygon_shape)
	self.polygon_2d.polygon = polygon_shape
