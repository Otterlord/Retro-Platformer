extends KinematicBody2D

# Physics variables

var spd = 600
var acc = 25
var frc = 25
var dec = 80
var grv = 40
var max_grv = 600
var slp = 30
var jmp_height = 800
var min_jmp

# References

var velocity = Vector2(0, 0)
var rot_spd = .2

var rot = 0
var gsp = 0
var vsp = 0
var jumping = false

var mode = 0
var offset = Vector2(0, 32)

onready var sprite = get_node("Sprite")
onready var sensors = get_node("Sensors")
onready var hitbox = get_node("HitBox")
onready var sensor_l = get_node("Sensors/LSensor")
onready var sensor_r = get_node("Sensors/RSensor")
onready var sensor_c = get_node("Sensors/CSensor")

var floor_normal = Vector2(0, -1)


func _ready():
	sensor_c.add_exception(self)
	sensor_l.add_exception(self)
	sensor_r.add_exception(self)

func ground_move():
	vsp = 0
	gsp += slp * sin(rot)
	if Input.is_action_just_pressed("jump"):
		vsp = -jmp_height
		jumping = true
	
	if Input.is_action_pressed("move_left"):
		if gsp > 0:
			gsp -= dec
		else:
			gsp -= acc
	elif Input.is_action_pressed("move_right"):
		if gsp < 0:
			gsp += dec
		else:
			gsp += acc
	else:
		if abs(gsp) < frc:
			gsp = 0
		else:
			gsp -= frc * sign(gsp)
	if abs(gsp) > spd:
		gsp = spd * sign(gsp)

func air_move():
	if Input.is_action_pressed("move_left"):
		gsp -= acc * 2
	elif Input.is_action_pressed("move_right"):
		gsp += acc * 2
	else:
		if vsp < 0 and vsp > -500:
			gsp *= 0.7658
	if abs(gsp) > spd:
		gsp = spd * sign(gsp)
	
	if jumping and vsp > 0:
		jumping = false
	
	vsp += grv
	if vsp > max_grv:
		vsp = max_grv

func _process(delta):
	floor_normal = get_floor_normal()
	rot = -floor_normal.angle_to(Vector2(0, -1))
	if sensor_c.is_colliding():
		rotation = -sensor_c.get_collision_normal().angle_to(Vector2(0,-1))
		#sensors.rotation = -sensor_c.get_collision_normal().angle_to(Vector2(0,-1))
		#sprite.rotation = lerp(sprite.rotation, -sensor_c.get_collision_normal().angle_to(Vector2(0,-1)), rot_spd)
	else:
		rotation = rot
		#sensors.rotation = rot
		#sprite.rotation = lerp(sprite.rotation, rot, rot_spd)
	var angle = rad2deg(rot)
	if mode == 0:
		if angle > 45:
			mode = 1
		elif angle < -45:
			mode = 3
	elif mode == 1:
		if angle < 45:
			mode = 0
		elif angle > 90:
			mode = 2
	elif mode == 2:
		if angle < 90:
			mode = 1
		elif angle > -90:
			mode = 3
	elif mode == 3:
		if angle > -45:
			mode = 0
		elif angle < -90:
			mode = 2
	
	# react to mode
	if mode == 0:
		#sensors.rotation = 0
		#hitbox.rotation = 0
		offset = Vector2(0, 32)
	elif mode == 1:
		#sensors.rotation = 90
		#hitbox.rotation = 90
		offset = Vector2(-32, 0)
	elif mode == 2:
		#sensors.rotation = 180
		#hitbox.rotation = 180
		offset = Vector2(0, -32)
	elif mode == 3:
		#sensors.rotation = -90
		#hitbox.rotation = -90
		offset = Vector2(32, 0)

func _physics_process(delta):
	velocity = Vector2(0, 0)
	
	if is_on_floor():
		ground_move()
	else:
		air_move()
	if vsp < 0:
		velocity = Vector2(0, vsp).rotated(rot)
	else:
		velocity = Vector2(0, vsp)
	
	velocity += Vector2(gsp, 0).rotated(rot)
	if jumping:
		move_and_slide_with_snap(velocity, Vector2(0, 0), floor_normal)
	else:
		move_and_slide_with_snap(velocity, offset, floor_normal)

func get_floor_normal():
	if sensor_l.is_colliding() and sensor_r.is_colliding():
		if mode == 0:
			if (sensor_l.get_collision_point().y >= sensor_r.get_collision_point().y):
				return sensor_l.get_collision_normal()
			else:
				return sensor_r.get_collision_normal()
		elif mode == 1:
			if (sensor_l.get_collision_point().x >= sensor_r.get_collision_point().x):
				return sensor_l.get_collision_normal()
			else:
				return sensor_r.get_collision_normal()
		elif mode == 2:
			if (sensor_l.get_collision_point().y <= sensor_r.get_collision_point().y):
				return sensor_l.get_collision_normal()
			else:
				return sensor_r.get_collision_normal()
		elif mode == 3:
			if (sensor_l.get_collision_point().x <= sensor_r.get_collision_point().x):
				return sensor_l.get_collision_normal()
			else:
				return sensor_r.get_collision_normal()
	elif sensor_l.is_colliding():
		return sensor_l.get_collision_normal()
	elif sensor_r.is_colliding():
		return sensor_r.get_collision_normal()
	else:
		return Vector2(0, -1)