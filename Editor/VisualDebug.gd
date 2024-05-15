extends Node
class_name VisualDebug

# press key to switch between debug render mode ( DebugDraw )

@export var KeySetup:Key = KEY_P
@export var enabled:bool = true

func _init():
	RenderingServer.set_debug_generate_wireframes(true)


func _input(event):
	if not enabled:
		return
	if event is InputEventWithModifiers:
		if Input.is_key_pressed(KeySetup) and event.is_command_or_control_pressed():
			var vp = get_viewport()
			vp.debug_draw = (vp.debug_draw+1) % 5
