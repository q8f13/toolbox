extends Node2D

var pid:PIDCtrl

@export var unit:GPUParticles2D

@export var target_y := 0.0

@export var speed := 2.0

@export var kp := 0.1
@export var ki := 0.1
@export var kd := -0.1

@export var use_pi:bool = false:
	set(val):
		use_pi = val
		if pid:
			pid.flag_use_PI = val
			
@export var use_pd:bool = false:
	set(val):
		use_pd = val
		if pid:
			pid.flag_use_PD = val


var _kp:float
var _ki:float
var _kd:float


func _ready() -> void:
	_kp = kp
	_ki = ki
	_kd = kd
	pid = PIDCtrl.new()
	
	
func _process(delta: float) -> void:
	if pid == null:
		return
	if _kp != kp or _ki != ki or _kd != kd:
		_kp = kp
		_ki = ki
		_kd = kd
		pid.kd = _kd
		pid.ki = _ki
		pid.kp = _kp
	
	var u = pid.tick(unit.position.y, target_y)
	unit.translate(Vector2(0.0, u))
	# add noise
	var rng = randf_range(3.0, 10.0)
	unit.position = Vector2(unit.position.x, unit.position.y + rng)
