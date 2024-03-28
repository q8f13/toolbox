shader_type spatial;
render_mode depth_draw_never, cull_front, shadows_disabled, blend_mix, unshaded;

#include "sdf.gdshaderinc"

//#define TINT_COLOR;

const int MAX_MARCHING_STEPS = 512;
const float MIN_DIST = 0.0;
//const float MAX_DIST = 200.0;
const float EPSILON = 0.0001;

uniform uint sample_times_through = 5;
uniform float multiplier:hint_range(0.0, 1.0, 0.02) = 0.2;
uniform float max_dist = 200.0;

uniform bool remap_for_tint = false;

uniform sampler2D color_tint_tex;

uniform float size = 4.0;

uniform vec3 uv_scale = vec3(1.0);
uniform vec3 uv_offset = vec3(0.0);

//uniform vec3 boundsMin;
uniform sampler3D noiseTex3D;

uniform float sampleThreshold:hint_range(0.0, 1.0, 0.02) = 0.1;

uniform float sphere_r = 1.3;

uniform vec3 boundsMax;
vec2 rayBoxDst(vec3 bMin, vec3 bMax, vec3 rPos, vec3 invRayDir){
	// Adapted from: http://jcgt.org/published/0007/03/04/
	vec3 t0 = (bMin - rPos) * invRayDir;
	vec3 t1 = (bMax - rPos) * invRayDir;
	vec3 tmin = min(t0, t1);
	vec3 tmax = max(t0, t1);

	float dstA = max(max(tmin.x, tmin.y), tmin.z);
	float dstB = min(tmax.x, min(tmax.y, tmax.z));

	// CASE 1: ray intersects box from outside (0 <= dstA <= dstB)
	// dstA is dst to nearest intersection, dstB dst to far intersection

	// CASE 2: ray intersects box from inside (dstA < 0 < dstB)
	// dstA is the dst to intersection behind the ray, dstB is dst to forward intersection

	// CASE 3: ray misses box (dstA > dstB)

	float dstToBox = max(0, dstA);
	float dstInsideBox = max(0, dstB - dstToBox);
	return vec2(dstToBox, dstInsideBox);
}


float sdSphere(vec3 p, float r){
	return length(p) - r;
}


float sceneSDF(vec3 eye, vec3 dir){
//	return sdSphere(eye, sphere_r);
//	return sphereSDF(eye);
	return sdBox(eye, boundsMax);
//	return rayBoxDst(boundsMin, boundsMax, eye, dir).x;
}

float expo(float v){
	return 0.002 * exp(v);
}

vec4 sampleVolume(vec3 p){
	vec3 uvw = (p / size + vec3(0.5)) * uv_scale + uv_offset;
//	vec3 uvw = p / 128.0 + vec3(0.5);
	uvw = clamp(uvw, vec3(0.0), vec3(1.0));
	vec4 v = texture(noiseTex3D, uvw);
	return v;
}

//const float SAMPLE_TIMES_THROUGH = 3.0;
float marchThrough(vec3 eye, vec3 marchingDirection, float start, float end, vec2 uv){
	float depth = start;
	float depth_begin = start;
	float depth_end;
	
	float depth_rev = 0.0;
	int times = int(sample_times_through);
	bool through = false;
	for(int i=0;i<MAX_MARCHING_STEPS;i++){
		float dist = sceneSDF(eye + depth * marchingDirection, marchingDirection);
		if(dist < EPSILON){
			depth_begin = depth;
			through = true;
			break;
		}
		depth += dist;
		if(depth > end * 2.0)
			return 0.0;
	}
	
//	// reverse
//	vec3 eye_rev = eye + marchingDirection * end * 1.0;
//	for(int i=0;i<MAX_MARCHING_STEPS;i++){
//		float dist = sceneSDF(eye_rev + depth_rev * -marchingDirection, -marchingDirection);
//		if(dist < EPSILON){
//			depth_end = depth_rev;
//			break;
//		}
//		depth_rev += dist;
//	}
	
	if(depth_end < 1.0)
		return 0.0;
	
	float accu = 0.0;
	vec3 p;
	float sample_through_step = (end - depth_begin) / float(sample_times_through);
	for(int i=0;i<int(times);i++){
		p = eye + marchingDirection * (depth_begin + sample_through_step * float(i));
		float sampled_val = sampleVolume(p).r;
		float val = step(sampleThreshold,sampled_val);
		accu += val * multiplier;
//		accu += expo(val);
	}
	return accu;
}

float shortestDistanceToSurface(vec3 eye, vec3 marchingDirection, float start, float end){
	float depth = start;
	for(int i=0;i<MAX_MARCHING_STEPS;i++){
		float dist = sceneSDF(eye + depth * marchingDirection, marchingDirection);
		if(dist < EPSILON){
			return depth;
		}
		depth += dist;
		if(depth >= end){
			return end;
		}
	}
	return end;
}

vec3 tint(float val){
	float a = val;
	if(remap_for_tint)
		a = val * 2.0 - 1.0;
	return texture(color_tint_tex, vec2(a, 0.0)).xyz;
}

void fragment() {
	vec3 rayPos = (INV_VIEW_MATRIX * vec4(vec3(0.0), 1.0)).xyz;
	float viewLength = length(CAMERA_DIRECTION_WORLD);
//	vec3 rayDir = CAMERA_DIRECTION_WORLD;
	vec3 rayDir = -(INV_VIEW_MATRIX * vec4(vec3(0.0, 0.0, 1.0), 1.0)).xyz;
	
	const float BOUND_SCALE = 500.0;
	vec3 p = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	vec3 viewDir = normalize(p - rayPos);

//	vec2 boxResult = rayBoxDst(boundsMin, boundsMax, p, viewDir);

//	vec3 entryPoint = rayPos + rayDir * boxResult.x;
//	float dist = 0.0;
	bool facing_front = dot(rayDir, viewDir) > 0.0;
	float dist = marchThrough(rayPos, viewDir, MIN_DIST, max_dist, UV);
#ifdef TINT_COLOR
	vec4 clr = vec4(tint(dist), dist);
#else
//	float dist = marchThrough(rayPos, viewDir, MIN_DIST, MAX_DIST, UV);
//	vec4 clr = vec4(0.0);
//	if(dist > MAX_DIST - EPSILON){
//		// hit nothing
//		clr = vec4(0.0);
//		discard;
//	}
//	clr = vec4(1.0, 0.0, 0.0, 1.0);

	vec4 clr = vec4(vec3(1.0), dist);
#endif
	
//	ALBEDO.xyz = p;
	ALBEDO.xyz = clr.xyz;
//	ALBEDO.xyz = vec3(dot(rayDir, viewDir));
	
	ALPHA = clamp(clr.a, 0.0, 1.0);
//	ALPHA = 1.0 - clr.a;
}
