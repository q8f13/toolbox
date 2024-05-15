extends RefCounted

class_name PIDCtrl


var flag_use_PI := false
var flag_use_PD := false

var _accum_val:float = 0.0

var _last_err:float = NAN

var kp:float = 0.5
var ki:float = 0.2
var kd:float = -0.2

func tick(curr:float, target_val:float)->float:
	var err:float = target_val - curr
	var u:float = kp * err
	if flag_use_PI:
		u += ki * _accum_val
		_accum_val += err
	if flag_use_PD:
		var d = 0.0
		if _last_err != NAN:
			d = kd*(u - _last_err)
			u+=d
		_last_err = err
	return u



func reset():
	_last_err = NAN
	_accum_val = 0.0
