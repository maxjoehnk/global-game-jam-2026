extends Node2D


const DefaultColor: Color  = Color(0.8, 0.0, 0.224, 0.392) 
const HoverColor: Color    = Color(0.025, 0.522, 0.0, 0.196) 

@onready var area_2d = $Area2D
@onready var sprite_2d = $Sprite2D
var physics_body : CutableRidgidBody = null

func _ready():
	self.area_2d.connect("body_entered", self._on_area_2d_body_entered)
	self.sprite_2d.modulate = self.DefaultColor

func _physics_process(_delta):
	if self.physics_body != null and Input.is_action_just_pressed("cut"):
		self.physics_body = null


func _on_area_2d_body_entered(body):
	print(body)
