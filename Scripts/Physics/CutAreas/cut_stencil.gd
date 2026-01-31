extends Node2D

const CutBody: PackedScene = preload("res://Scenes/Physics/cutable_ridgid_body.tscn")
const DefaultColor: Color  = Color(0.8, 0.0, 0.224, 0.392) 
const HoverColor: Color    = Color(0.025, 0.522, 0.0, 0.196) 

@onready var sprite_2d : Sprite2D = $Sprite2D
@onready var collision_polygon_2d : CollisionPolygon2D = $Area2D/CollisionPolygon2D

var physics_body : CutableRidgidBody = null

func _ready():
	self.sprite_2d.modulate = self.DefaultColor

func _physics_process(_delta):
	self.global_position = self.get_global_mouse_position()
	if self.physics_body != null and Input.is_action_just_pressed("cut"):
		self.cut_intersection()
		self.physics_body = null


func _on_area_2d_body_entered(body):
	if is_instance_of(body, CutableRidgidBody):
		self.physics_body = body
		self.sprite_2d.modulate = self.HoverColor


func _on_area_2d_body_exited(_body):
	self.physics_body = null
	self.sprite_2d.modulate = self.DefaultColor


func cut_intersection():
	# First cut in one direction (removing the zone stencil from the 
	# rigidbody)
	var array1: PackedVector2Array = self.shift_polygon_vec(
		self.physics_body.global_position, 
		self.physics_body.global_rotation,
		self.physics_body.collision_polygon_2d.polygon)
	var array2: PackedVector2Array = self.shift_polygon_vec(
		self.global_position, 
		0.0,
		self.collision_polygon_2d.polygon)
	
	#self.build_new_objects(array1, array2)
	var new_polygons: Array[PackedVector2Array] = Geometry2D.clip_polygons(array1, array2)
	self.build_new_object(new_polygons)
	new_polygons = Geometry2D.intersect_polygons(array2, array1)
	self.build_new_object(new_polygons, false)
	
	self.physics_body.queue_free()


func build_new_object(new_polygons, freeze_object: bool = true):
	var parent: Node = self.physics_body.get_parent()
	for poly_new in new_polygons:
		var point_center: Vector2 = points_center(poly_new)
		poly_new = self.shift_polygon_vec(-point_center, 0.0, poly_new)
		var new_body: Node = self.CutBody.instantiate()
		parent.add_child(new_body)
		new_body.global_position = point_center
		new_body.set_shape(poly_new)
		self.physics_body.copy_texture_data(new_body)
		if not freeze_object:
			new_body.freeze = false


func points_center(points: PackedVector2Array) -> Vector2:
	var center := Vector2.ZERO
	for p in points:
		center += p
	return center / points.size()


func shift_polygon_vec(global_pos: Vector2, angle: float,
						poly_vertices: PackedVector2Array) -> PackedVector2Array:
	var array: PackedVector2Array = PackedVector2Array()
	for vec in poly_vertices:
		array.append(vec.rotated(angle) + global_pos)
	return array
