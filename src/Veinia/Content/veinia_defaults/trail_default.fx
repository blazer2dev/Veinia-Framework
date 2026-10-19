#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;
float4 GlobalColor;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
    float Visibility : TEXCOORD1;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, WorldViewProjection);

    // TrailVertex.Visibility is already a normalized opacity value. Keeping
    // the fade in alpha lets the render state's alpha blending do the work.
    float visibility = saturate(input.Visibility);
    output.Color = float4(GlobalColor.rgb, GlobalColor.a * visibility);

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return input.Color;
}

technique AmbientTrail
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
