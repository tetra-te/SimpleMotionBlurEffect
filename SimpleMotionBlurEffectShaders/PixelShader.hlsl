Texture2D InputTexture : register(t0);
SamplerState InputSampler : register(s0);

cbuffer constants : register(b0)
{
	float blur : packoffset(c0.x);
}; 

float2 ClampVector(float2 v, float max)
{
    float len = length(v);
    return len > max ? v / len * max : v;
}

float4 main(
	float4 pos : SV_POSITION,
	float4 posScene : SCENE_POSITION,
	float4 uv0 : TEXCOORD0
) : SV_Target
{
    float2 v = posScene.xy;
    float2 dv = ClampVector(v * blur / 100, 4000);
    float samples = floor(max(1, length(dv)));

    float4 color = float4(0, 0, 0, 0);
    [loop]
    for (int i = 0; i < samples; i++)
    {
        color += InputTexture.SampleLevel(InputSampler, uv0.xy - dv * i / samples * uv0.zw, 0);
    }
    color /= samples;

    return color;
}