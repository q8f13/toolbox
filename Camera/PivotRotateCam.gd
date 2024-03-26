extends Camera3D
class_name PivotRotateCam

@export_category("basic")
@export var target:Node3D;
@export var scroll_power = 0.8

@export_category("limits")
@export var pitch_max_deg = 60.0
@export var zoom_distance_max = 300.0

@export_category("easing")
@export var rotation_dt_multiplier = 5.0


var mouse_pressing = Vector2()
var mouse_last = Vector2()

# flags
var pivoting = false
var rot_pitching = false

var arm_distance:float
var arm_dir:Vector3
var _pending_dir:Vector3
var zoom_var := 0.0;

var speed:float = 5.0

var dd_screen = Vector2()


var rotation_dt = 0.0

var mouse_dd_scale = 0.2


# Called when the node enters the scene tree for the first time.
func _ready():
	if target == null:
		printerr("invalid target")
		return

	reset()



func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		if event.is_pressed():
			match event.button_index:
				MOUSE_BUTTON_WHEEL_UP:
					zoom_var += scroll_power
				MOUSE_BUTTON_WHEEL_DOWN:
					zoom_var -= scroll_power
				MOUSE_BUTTON_LEFT:
					mouse_pressing = event.position
					pivoting = true
					dd_screen.x=0
					dd_screen.y=0

		else:
			match event.button_index:
				MOUSE_BUTTON_LEFT:
					pivoting = false
					
		zoom_var = min(zoom_var, zoom_distance_max)


			
	
	if event is InputEventMouseMotion:		
		mouse_last = event.position
		if pivoting:
			dd_screen = event.relative * mouse_dd_scale
			
			
	if event is InputEventKey:
		if event.is_pressed():
			match event.keycode:
				KEY_W,KEY_A,KEY_D,KEY_S:
					_move_cam(event.keycode)
					pass
				_:
					pass
			
	pass


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
#	print('press: %s, last: %s, dd_screen.x:%s, panning: %s, pitch: %s, zoom: %s' % [mouse_pressing, mouse_last, dd_screen.x, panning, pitch, zoom_var])

	dd_screen.x*=0.9
	dd_screen.y*=0.9
	
#	dd_screen.x = 0.0
#	dd_screen.y = 0.0

	if pivoting:
		# rotate arm
		var dir = _pending_dir
		dir = Quaternion(self.global_transform.basis.y, delta * -dd_screen.x) * Quaternion(self.global_transform.basis.x, delta*-dd_screen.y) * dir
		
		# pitch limit
		var dir_projected := dir.slide(Vector3.UP).normalized()
		var axis := dir.cross(dir_projected).normalized()
		var pitch_angle := rad_to_deg(dir_projected.signed_angle_to(dir, axis))
		var sign = sign(pitch_angle)
		if abs(pitch_angle) > pitch_max_deg and sign != 0.0:
			dir = dir_projected.rotated(axis, deg_to_rad(sign * pitch_max_deg))
		
		# apply to pending
		_pending_dir = dir

	arm_dir = arm_dir.slerp(_pending_dir, delta * rotation_dt_multiplier)
	
	var tar_pos = target.global_position + arm_dir * (arm_distance + zoom_var)

	# do notice that position first is important
	self.global_position = tar_pos
	self.global_transform = self.global_transform.looking_at(target.global_position)
	pass


func reset():
	arm_distance = (target.transform.origin - self.transform.origin).length()
	arm_dir = (self.global_position - target.global_position).normalized()
	_pending_dir = arm_dir


func _move_cam(keycode:int):
	var dir = Vector3.ZERO
	match keycode:
		KEY_W:
			dir = -self.transform.basis.z
		KEY_A:
			dir = -self.transform.basis.x
		KEY_S:
			dir = self.transform.basis.z
		KEY_D:
			dir = self.transform.basis.x
	if dir != Vector3.ZERO:
		dir = dir.slide(Vector3.UP).normalized()
		var offset = (dir * get_process_delta_time() * speed)
		target.global_position += offset
#		self.global_translate(offset)
		pass
	pass
