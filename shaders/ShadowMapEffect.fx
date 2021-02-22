//transformations
float4x4 WorldViewProjection : WORLDVIEWPROJECTION;
float4x4 MatrixPalette[56];
float4x4 World : WORLD;
shared float4x4 View : VIEW;
shared float4x4 Projection : PROJECTION;
shared float3 EyePosition : CAMERAPOSITION;

//SpotLight Properties
struct Light
{
    float4x4 View;
    float4x4 Projection;
    float3 Position;
    float3 Color;
};

//shared parameters
shared Light Lights[2];
shared int ActiveLightCount;
shared float3 AmbientLightColor;

//Material Properties
float3 DiffuseColor : DIFFUSE; 
float Alpha : OPACITY;
float3 SpecularColor : SPECULAR;
float SpecularPower : SPECULARPOWER;
float3 EmissiveColor : EMISSIVE;

//Texture Settings
Texture BasicTexture : TEXTURE0;
bool TextureEnabled;

//Shadow Settings
float ShadowMapDepthBias = 0.001f;
//float ShadowFactor = 0.75f;

struct ColorPair
{
	float3 Diffuse;
	float3 Specular;
};

ColorPair ComputeSingleLight(float3 lightColor,
                          float3 negLightDir,
                          float3 eyeVector,
                          float3 pixelNormal)
{
	ColorPair result;
	
	result.Diffuse = 0;
	result.Specular = 0;
	
	float3 eyePlusNegLightDir = normalize(eyeVector + negLightDir);
	float dt = max(0, dot(negLightDir, pixelNormal));
    result.Diffuse += lightColor * dt;
    if (dt != 0)
		result.Specular += (lightColor * pow(saturate(dot(eyePlusNegLightDir, pixelNormal)), SpecularPower));

    result.Diffuse += EmissiveColor;
    result.Specular *= SpecularColor;
		
	return result;
}

sampler TextureSampler = sampler_state 
{ 
    texture = <BasicTexture>; 
    magfilter = LINEAR; 
    minfilter = LINEAR; 
    mipfilter = LINEAR; 
};

struct VertexToPixel
{
    float4 Position : POSITION;
    float4 Position3D : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float2 TexCoord : TEXCOORD2;
};

VertexToPixel PerPixelLightingVertexShader(float4 inPos : POSITION,
                                  float2 inTexCoord : TEXCOORD0,
                                  float3 inNormal : NORMAL)
{
    VertexToPixel Output = (VertexToPixel)0;

    WorldViewProjection = mul(mul(World, View), Projection);
    Output.Position = mul(inPos, WorldViewProjection);    
    Output.Normal = normalize(mul(inNormal, (float3x3)World));
    Output.TexCoord = inTexCoord;

    return Output;
}

struct PixelToFrame
{
    float4 Color : COLOR0;
};

PixelToFrame PerPixelLightingPixelShader(VertexToPixel pixelInput)
{
    PixelToFrame Output = (PixelToFrame)0;

    if(TextureEnabled)
        Output.Color = tex2D(TextureSampler, pixelInput.TexCoord) * float4(DiffuseColor, Alpha);
    else
        Output.Color = float4(DiffuseColor, Alpha);

    float3 eyeVector = normalize(EyePosition - pixelInput.Position3D);

    ColorPair lighting;
    lighting.Diffuse = 0.0f;
    lighting.Specular = 0.0f;
    for(int i = 0; i < ActiveLightCount; ++i)
    {
        float3 negLightDir = -normalize(pixelInput.Position3D - Lights[i].Position);
        ColorPair single = ComputeSingleLight(Lights[i].Color,
                                       negLightDir, 
                                       eyeVector, 
                                       pixelInput.Normal);
        lighting.Diffuse += single.Diffuse;
        lighting.Specular += single.Specular;
    }
    
    lighting.Diffuse = saturate(lighting.Diffuse);
    
    float4 diffuse = (Output.Color * float4(lighting.Diffuse + AmbientLightColor, 1.0f));
    Output.Color = diffuse + float4(lighting.Specular, 0.0f);

    return Output;
}

struct ShadowMapPixelShaderInput
{
    float4 Position : POSITION;
    float Depth : TEXCOORD0;
};

ShadowMapPixelShaderInput ShadowMapVertexShader(float4 inPos : POSITION)
{
    ShadowMapPixelShaderInput Output = (ShadowMapPixelShaderInput)0;

    //when running with this pixel shader the View and Projection
    //are set to the View and Projection of the current light
    WorldViewProjection = mul(mul(World, View), Projection);
    float4 pos = mul(inPos, WorldViewProjection);

    Output.Position = pos;
    Output.Depth = pos.z / pos.w;

    return Output;
}

struct ShadowMapPixelToFrame
{
    float4 Color : COLOR0;
};

ShadowMapPixelToFrame ShadowMapPixelShader(ShadowMapPixelShaderInput pixelInput)
{
    ShadowMapPixelToFrame Output = (ShadowMapPixelToFrame)0;

    Output.Color = float4(pixelInput.Depth, 0.0f, 0.0f, 1.0f);

    return Output;
}

shared Texture ShadowMap0;
shared Texture LightShapeTexture;
//shared Texture ShadowMap1;

sampler ShadowMapSampler0 = sampler_state 
{ 
    texture = <ShadowMap0>; 
    magfilter = LINEAR; 
    minfilter = LINEAR; 
    mipfilter = LINEAR; 
    AddressU = clamp; 
    AddressV = clamp;
};

sampler LightShapeSampler = sampler_state 
{ 
    texture = <LightShapeTexture>; 
    magfilter = LINEAR; 
    minfilter = LINEAR; 
    mipfilter = LINEAR; 
    AddressU = clamp; 
    AddressV = clamp;
};

/*sampler ShadowMapSampler1 = sampler_state 
{ 
    texture = <ShadowMap1>; 
    magfilter = LINEAR; 
    minfilter = LINEAR; 
    mipfilter = LINEAR; 
    AddressU = clamp; 
    AddressV = clamp;
};*/

struct ShadowedScenePixelShaderInput
{
    float4 Position : POSITION;
    float4 Position3D : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float2 TexCoord : TEXCOORD2;
    float4 Pos2DAsSeenByLight0 : TEXCOORD3;
    //float4 Pos2DAsSeenByLight1 : TEXCOORD5;
};

struct ShadowedScenePixelToFrame
{
    float4 Color : COLOR0;
};

float4 ComputeVtxPosRelativeToLight(float4 inPos, Light light)
{
    float4x4 lightWorldViewProjection = mul(mul(World, light.View), light.Projection);
    return mul(inPos, lightWorldViewProjection);
}

ShadowedScenePixelShaderInput ShadowedSceneVertexShader(float4 inPos : POSITION,
                                                 float2 inTexCoord : TEXCOORD0,
                                                 float3 inNormal : NORMAL)
{
    ShadowedScenePixelShaderInput Output = (ShadowedScenePixelShaderInput)0;

    //Light 0
    Output.Pos2DAsSeenByLight0 = ComputeVtxPosRelativeToLight(inPos, Lights[0]);
    
    //Light1
    //if(ActiveLightCount > 1)
    //{
    //    float4x4 lightWorldViewProjection1 = mul(mul(World, Lights[1].View), Lights[1].Projection);
    //    Output.Pos2DAsSeenByLight1 = mul(inPos, lightWorldViewProjection1);    
    //}

    WorldViewProjection = mul(mul(World, View), Projection);
    Output.Position = mul(inPos, WorldViewProjection);    
    Output.Normal = normalize(mul(inNormal, (float3x3)World));
    Output.Position3D = mul(inPos, World);
    Output.TexCoord = inTexCoord;

    return Output;
}

const float2 TexelSize = {1.0f/512.0f, 1.0f/512.0f};

float OffsetLookup(sampler2D map, float2 projTexUV, float2 offset)
{
	return tex2D(map, float2(projTexUV + (offset * TexelSize))).r;
}

ShadowedScenePixelToFrame ShadowedScenePixelShader(ShadowedScenePixelShaderInput pixelInput)
{
    ShadowedScenePixelToFrame Output = (ShadowedScenePixelToFrame)0;    
    
    if(TextureEnabled)
        Output.Color = tex2D(TextureSampler, pixelInput.TexCoord) * float4(DiffuseColor, Alpha);
    else
        Output.Color = float4(DiffuseColor, Alpha);

    float3 eyeVector = normalize(EyePosition - pixelInput.Position3D);

    ColorPair lighting;
    lighting.Diffuse = 0.0f;
    lighting.Specular = 0.0f; 
    
    float2 ProjectedTexCoord0;
    ProjectedTexCoord0.x = pixelInput.Pos2DAsSeenByLight0.x / pixelInput.Pos2DAsSeenByLight0.w / 2.0f + 0.5f;
    ProjectedTexCoord0.y = -pixelInput.Pos2DAsSeenByLight0.y / pixelInput.Pos2DAsSeenByLight0.w / 2.0f + 0.5f;
        
    //if the projected texture coords are between 0 and 1 then this pixel corresponds to a
    //pixel in the shadow map
    float2 sat = saturate(ProjectedTexCoord0);
    //if after the saturate call the projected texture coord is unchanged then we know
    //that is between 0 and 1
    if (sat.x == ProjectedTexCoord0.x && sat.y == ProjectedTexCoord0.y)
    {
        float3 negLightDir = -normalize(pixelInput.Position3D - Lights[0].Position);
        ColorPair single = ComputeSingleLight(Lights[0].Color,
                                       negLightDir, 
                                       eyeVector, 
                                       pixelInput.Normal);
        lighting.Diffuse += single.Diffuse;
        lighting.Specular += single.Specular;
    
        lighting.Diffuse = saturate(lighting.Diffuse);

        //float depthStoredInShadowMap = tex2D(ShadowMapSampler0, ProjectedTexCoord0).r;
        float depthStoredInShadowMap[10];
        depthStoredInShadowMap[0] = OffsetLookup(ShadowMapSampler0, ProjectedTexCoord0, float2(0.0f, 0.0f));
        depthStoredInShadowMap[1] = depthStoredInShadowMap[0];
        depthStoredInShadowMap[2] = OffsetLookup(ShadowMapSampler0, ProjectedTexCoord0, float2(1.0f, 0.0f));
        depthStoredInShadowMap[3] = OffsetLookup(ShadowMapSampler0, ProjectedTexCoord0, float2(0.0f, 1.0f));
        depthStoredInShadowMap[4] = OffsetLookup(ShadowMapSampler0, ProjectedTexCoord0, float2(-1.0f, 0.0f));
        depthStoredInShadowMap[5] = OffsetLookup(ShadowMapSampler0, ProjectedTexCoord0, float2(0.0f, -1.0f));
        depthStoredInShadowMap[6] = OffsetLookup(ShadowMapSampler0, ProjectedTexCoord0, float2(1.0f, 1.0f));
        depthStoredInShadowMap[7] = OffsetLookup(ShadowMapSampler0, ProjectedTexCoord0, float2(-1.0f, 1.0f));
        depthStoredInShadowMap[8] = OffsetLookup(ShadowMapSampler0, ProjectedTexCoord0, float2(1.0f, -1.0f));
        depthStoredInShadowMap[9] = OffsetLookup(ShadowMapSampler0, ProjectedTexCoord0, float2(-1.0f, -1.0f));
    
        float distToCurPixel = (pixelInput.Pos2DAsSeenByLight0.z / pixelInput.Pos2DAsSeenByLight0.w) - ShadowMapDepthBias;
        
        float shadowFactor = 0.0f;
        
        for(int i = 0; i < 10; ++i)
        {
	        shadowFactor += distToCurPixel > depthStoredInShadowMap[i] ? 0.05f : 0.1f;
        }
        //if the distance from the current pixel to the light is greater than the distance
        //stored in the shadow map then this pixel is shadowed
        //if (distToCurPixel > depthStoredInShadowMap)
        shadowFactor = saturate(shadowFactor);
        lighting.Diffuse *= shadowFactor;
        lighting.Specular *= shadowFactor;
        
        float lightShapeFactor = tex2D(LightShapeSampler, ProjectedTexCoord0).r;
        lighting.Diffuse *= lightShapeFactor;
        lighting.Specular *= lightShapeFactor;
        //lighting.Diffuse = saturate(lighting.Diffuse); 
    }

    float4 diffuse = (Output.Color * float4(lighting.Diffuse + float3(0.2f, 0.2f, 0.2f), 1.0f));
    Output.Color = diffuse;// + float4(lighting.Specular, 0.0f);
    return Output;
}

struct SkinnedVertexShaderInput
{
    float4 position : POSITION;
    float4 color : COLOR;
    float2 texcoord : TEXCOORD0;
    float3 normal : NORMAL0;
    half4 indices : BLENDINDICES0;
    float4 weights : BLENDWEIGHT0;
};

// This is passed out from our vertex shader once we have processed the input
struct VS_OUTPUT
{
    float4 position : POSITION;
    float4 color : COLOR;
    float2 texcoord : TEXCOORD0;
    float  distance : TEXCOORD1;
};

// This is the output from our skinning method
struct SkinnedVertex
{
    float4 position;
    float4 normal;
};

// This calculates the skinned vertex and normal position based on the blend indices
// and weights.
// For four indices and four weights, which is what this shader uses,
// the formula for position vertex Vi, weight array W, index array I, and matrix array M is:
// Vf = Vi*W[0]*M[I[0]] + Vi*W[1]*M[I[1]] + Vi*W[2]*M[I[2]] + Vi*W[3]*M[I[3]]
// In fact, the weights may not always add up to 1,
// so we replace the last weight with:
// W[3] = (1 - W[2] - W[1] - W[0])
// The formula is the same for calculating the skinned normal position.
SkinnedVertex Skin4( const SkinnedVertexShaderInput input)
{
    SkinnedVertex output = (SkinnedVertex)0;

    float lastWeight = 1.0;
    float weight = 0;
    for (int i = 0; i < 3; ++i)
    {
        weight = input.weights[i];
        lastWeight -= weight;
        output.position     += mul( input.position, MatrixPalette[input.indices[i]]) * weight;
        output.normal       += mul( input.normal  , MatrixPalette[input.indices[i]]) * weight;
    }
    output.position     += mul( input.position, MatrixPalette[input.indices[3]])*lastWeight;
    output.normal       += mul( input.normal  , MatrixPalette[input.indices[3]])*lastWeight;
    return output;
};

void SkinnedShadowMapVertexShader(in SkinnedVertexShaderInput input, out ShadowMapPixelShaderInput Output)
{
    SkinnedVertex skin = Skin4(input);
    
    //when running with this pixel shader the View and Projection
    //are set to the View and Projection of the current light
    WorldViewProjection = mul(World,mul(View,Projection));
    float4 pos = mul(skin.position, WorldViewProjection);
    Output.Position = pos;
    Output.Depth = pos.z / pos.w;
}

void SkinnedVertexShader(in SkinnedVertexShaderInput input, out ShadowedScenePixelShaderInput Output)
{
    // Calculate the skinned position
    SkinnedVertex skin = Skin4(input);
    // This is the final position of the vertex, and where it will be drawn on the screen
    WorldViewProjection = mul(World,mul(View,Projection));
    Output.Position = mul(skin.position, WorldViewProjection);
    Output.Position3D = mul(skin.position, World);
    Output.Normal = normalize(mul(skin.normal, (float3x3)World));
    Output.TexCoord = input.texcoord;
    Output.Pos2DAsSeenByLight0 = ComputeVtxPosRelativeToLight(skin.position, Lights[0]);
}

technique PerPixelLighting
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 PerPixelLightingVertexShader();
        PixelShader = compile ps_2_0 PerPixelLightingPixelShader();
    }
}

technique ShadowMap
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 ShadowMapVertexShader();
        PixelShader = compile ps_2_0 ShadowMapPixelShader();
    }
}

technique SkinnedShadowMap
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 SkinnedShadowMapVertexShader();
        PixelShader = compile ps_2_0 ShadowMapPixelShader();
    }
}

technique ShadowedScene
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 ShadowedSceneVertexShader();
        PixelShader = compile ps_3_0 ShadowedScenePixelShader();
    }
}

technique ShadowedSkinnedModel
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 SkinnedVertexShader();
        PixelShader = compile ps_3_0 ShadowedScenePixelShader();
    }
}

technique SkinnedPerPixelLighting
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 SkinnedVertexShader();
        PixelShader = compile ps_2_0 PerPixelLightingPixelShader();
    }
}
