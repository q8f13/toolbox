// qfbox shader utils
// a simple collection for shader snippets

// Get fresnel R
// https://kylehalladay.com/blog/tutorial/2014/02/18/Fresnel-Shaders-From-The-Ground-Up.html
float CalcFresnel(float bias, float scale, float power, float3 wpos, float3 eye, float3 wNormal)
{
	float3 I = normalize(wpos - eye);
	return bias + scale * pow(1.0 + dot(I, wNormal), power);
}