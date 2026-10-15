#if OPENGL
#define PS_PROFILE ps_3_0
#else
#define PS_PROFILE ps_4_0
#endif

texture SpriteTexture;

sampler SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

float FlashAmount = 0.0f;
float4 FlashColor = float4(1, 1, 1, 1);

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float4 texColor = tex2D(SpriteTextureSampler, input.TextureCoordinates) * input.Color;

    float amount = saturate(FlashAmount);

    float3 flashRGB = FlashColor.rgb * texColor.a;

    float3 result = lerp(texColor.rgb, flashRGB, amount);

    return float4(result, texColor.a);
}

technique SpriteDrawing
{
    pass Pass0
    {
        PixelShader = compile PS_PROFILE MainPS();
    }
}