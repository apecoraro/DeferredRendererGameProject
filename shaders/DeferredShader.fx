float4x4 WorldViewProjection : WORLDVIEWPROJECTION;
float4x4 MatrixPalette[56];
float4x4 World;
shared float4x4 View;
shared float4x4 Projection;
//position of the camera, for specular light
shared float3 CameraPosition; 
//this is used to compute the world-position
shared float4x4 InverseViewProjection; 

//constants - TODO these should be set by the model
float SpecularIntensity = 0.8f;
float SpecularPower = 0.5f;

//TODO same with this
texture DiffuseTextureMap;
sampler DiffuseTextureMapSampler = sampler_state
{    
    Texture = (DiffuseTextureMap);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct GBufferVSInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct GBufferVSOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float2 Depth : TEXCOORD2;
};

GBufferVSOutput GBufferVertexShader(GBufferVSInput input)
{
    GBufferVSOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.TexCoord = input.TexCoord; //pass the texture coordinates further
    output.Normal = normalize(mul(input.Normal, (float3x3)World)); //get normal into world space
    output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w; 

    return output;
}

struct GBufferPSOutput
{    
    float4 Color : COLOR0;
    float4 Normal : COLOR1;
    float4 Depth : COLOR2;
};

GBufferPSOutput GBufferPixelShader(GBufferVSOutput input)
{
    GBufferPSOutput output;
    output.Color = tex2D(DiffuseTextureMapSampler, input.TexCoord); //output Color
    output.Color.a = SpecularIntensity; //output SpecularIntensity
    output.Normal.rgb = 0.5f * (input.Normal + 1.0f); //transform normal domain
    output.Normal.a = SpecularPower; //output SpecularPower
    output.Depth = input.Depth.x / input.Depth.y;

    return output;
}

technique GBufferTechnique
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 GBufferVertexShader();
        PixelShader = compile ps_2_0 GBufferPixelShader();
    }
}

struct ClearGBufferVSInput
{
    float3 Position : POSITION0;
};

struct ClearGBufferVSOutput
{
    float4 Position : POSITION0;
};

ClearGBufferVSOutput ClearGBufferVertexShader(ClearGBufferVSInput input)
{
    ClearGBufferVSOutput output;
    output.Position = float4(input.Position,1);
    return output;
}
struct ClearGBufferPSOutput
{
    float4 Color : COLOR0;
    float4 Normal : COLOR1;
    float4 Depth : COLOR2;
};

ClearGBufferPSOutput ClearGBufferPixelShader(ClearGBufferVSOutput input)
{
    ClearGBufferPSOutput output;
    //black color
    output.Color = 0.0f;
    output.Color.a = 0.0f;
    //when transforming 0.5f into [-1,1], we will get 0.0f
    output.Normal.rgb = 0.5f;
    //no specular power
    output.Normal.a = 0.0f;
    //max depth
    output.Depth = 1.0f;
    return output;
}

technique ClearGBufferTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 ClearGBufferVertexShader();
        PixelShader = compile ps_2_0 ClearGBufferPixelShader();
    }
}

//struct SpotLight
//{
//    float4x4 View;
//    float4x4 Projection;
//    float3 Position;
//    float3 Color;
//    float3 Direction;
//    float2 ConeAngleAndDecay;
//};

//Directional, Point, and Spot light properties
//direction of the light
float3 LightDirection;
//color of the light 
float3 LightColor; 
//Shadow Map Matrix - transforms from world space to light projection space
float4x4 ShadowMapWarpMatrix;
//Shadow Map Jitter lookup
float4 ShadowJitter0;
float4 ShadowJitter1;
float4 ShadowJitter2;
float4 ShadowJitter3;
//Point and Spot properties
float3 LightPosition;
//control the brightness of the light
float LightIntensity = 1.0f;
//how far does this light reach
float LightRadius;
//outer angle of spot light
float SpotLightCosOuterCone;
//inner angle of spot light
float SpotLightCosInnerCone;
// diffuse color, and specularIntensity in the alpha channel
texture ColorMap; 
// normals, and specularPower in the alpha channel
texture NormalMap;
//depth
texture DepthMap;

sampler ColorSampler = sampler_state
{    
    Texture = (ColorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

sampler DepthSampler = sampler_state
{    
    Texture = (DepthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

sampler NormalSampler = sampler_state
{
    Texture = (NormalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

texture ShadowMap;

sampler ShadowMapSampler = sampler_state 
{
	Texture = (ShadowMap);
	magfilter = LINEAR; 
    minfilter = LINEAR; 
    mipfilter = LINEAR; 
    AddressU = CLAMP; 
    AddressV = CLAMP;
};

struct DirectionalLightShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct DirectionalLightShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

float2 HalfPixel;

DirectionalLightShaderOutput ScreenAlignedQuadVertexShader(DirectionalLightShaderInput input)
{
    DirectionalLightShaderOutput output;
    output.Position = float4(input.Position, 1.0f);
    //align texture coordinates
    output.TexCoord = input.TexCoord - HalfPixel;
    return output;
}

void LookUpNormalAndSpecularParams(float2 texCoord, 
                                   out float3 normal, 
                                   out float2 specularParams)
{
    //get normal data from the normalMap    
    float4 normalData = tex2D(NormalSampler, texCoord);
    //tranform normal back into [-1,1] range
    normal = 2.0f * normalData.xyz - 1.0f;
    //get specular power, and get it into [0,255] range]
    specularParams.x = normalData.a * 255;
    //get specular intensity from the colorMap
    specularParams.y = tex2D(ColorSampler, texCoord).a;
}

void ComputePositionFromDepthMap(float2 texCoord,
                                 out float4 position)
{
    //read depth
    float depthVal = tex2D(DepthSampler, texCoord).r;
    //compute screen-space position
    position.x = texCoord.x * 2.0f - 1.0f;
    position.y = -(texCoord.y * 2.0f - 1.0f);
    position.z = depthVal;
    position.w = 1.0f;
    //transform to world space
    position = mul(position, InverseViewProjection);
    position /= position.w;
}

void ComputeDiffuseLight(float3 normal, float3 lightVector, out float3 diffuseLight)
{
    float NdL = max(0,dot(normal, lightVector));
    diffuseLight = NdL * LightColor.rgb;
}

float ComputeSpecularLight(float4 position, float3 reflectionVector, float2 specularParams)
{
    //camera-to-surface vector
    float3 directionToCamera = normalize(CameraPosition - position);
    //compute specular light
    return specularParams.y * pow(saturate(dot(reflectionVector, directionToCamera)), specularParams.x);
}

float4 DirectionalLightPixelShader(DirectionalLightShaderOutput input) : COLOR0
{
    //surface-to-light vector
    float3 lightVector = -normalize(LightDirection);
    float3 normal;
    float2 specularParams;
    LookUpNormalAndSpecularParams(input.TexCoord, normal, specularParams);

    //compute diffuse light
    float3 diffuseLight;
    ComputeDiffuseLight(normal, lightVector, diffuseLight);

    float4 position;
    ComputePositionFromDepthMap(input.TexCoord, position);
    
    //reflexion vector
    float3 reflectionVector = normalize(reflect(-lightVector, normal));
    //camera-to-surface vector
    float specularLight = ComputeSpecularLight(position, reflectionVector, specularParams);
    return float4(diffuseLight.rgb, specularLight) ;
}

technique DirectionalLightTechnique
{    
    pass Pass0
    {
        VertexShader = compile vs_2_0 ScreenAlignedQuadVertexShader();
        PixelShader = compile ps_2_0 DirectionalLightPixelShader();    
    }
}

struct ShapedLightVertexShaderInput
{
    float3 Position : POSITION0;
};

struct ShapedLightVertexShaderOutput
{
    float4 Position : POSITION0;
    float4 ScreenPosition : TEXCOORD0;
};

ShapedLightVertexShaderOutput ShapedLightVertexShader(ShapedLightVertexShaderInput input)
{
    ShapedLightVertexShaderOutput output;
    //processing geometry coordinates
    float4 worldPosition = mul(float4(input.Position,1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    //why make a copy?
    output.ScreenPosition = output.Position;
    return output;
}

float4 PointLightPixelShader(ShapedLightVertexShaderOutput input) : COLOR0
{
    //obtain screen position
    input.ScreenPosition.xy /= input.ScreenPosition.w;
    //obtain textureCoordinates corresponding to the current pixel
    //the screen coordinates are in [-1,1]*[1,-1]
    //the texture coordinates need to be in [0,1]*[0,1]
    float2 texCoord = 0.5f * 
                  (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);
    //allign texels to pixels
    texCoord -= HalfPixel;

    float3 normal;
    float2 specularParams;
    LookUpNormalAndSpecularParams(texCoord, normal, specularParams);

    //compute world space position
    float4 position;
    //read depth
    float depthVal = tex2D(DepthSampler, texCoord).r;
    //compute screen-space position
    position.xy = input.ScreenPosition.xy;
    position.z = depthVal;
    position.w = 1.0f;
    //transform to world space
    position = mul(position, InverseViewProjection);
    position /= position.w;

    //surface-to-light vector
    float3 lightVector = LightPosition - position;
    //compute attenuation based on distance - linear attenuation
    float ratio = length(lightVector) / LightRadius;
    float attenuation = saturate(1.0f - (ratio * ratio));
    //normalize light vector
    lightVector = normalize(lightVector);
    //compute diffuse light
    float3 diffuseLight;
    ComputeDiffuseLight(normal, lightVector, diffuseLight);

    //reflection vector
    float3 reflectionVector = normalize(reflect(-lightVector, normal));
    float specularLight = ComputeSpecularLight(position, reflectionVector, specularParams);

    //take into account attenuation and LightIntensity.
    return attenuation * LightIntensity * float4(diffuseLight.rgb, specularLight);
}

technique PointLightTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 ShapedLightVertexShader();
        PixelShader = compile ps_2_0 PointLightPixelShader();
    }
}

const float TexelSize = 1.0f/512.0f;//, 1.0f/1024.0f, 0.0f, 0.0f};

float OffsetLookup(sampler2D map, float4 projTexUV)
{
	return tex2Dproj(map, projTexUV).r;
}

float4 SpotLightPixelShader(ShapedLightVertexShaderOutput input) : COLOR0
{
    //obtain screen position
    input.ScreenPosition.xy /= input.ScreenPosition.w;
    //obtain textureCoordinates corresponding to the current pixel
    //the screen coordinates are in [-1,1]*[1,-1]
    //the texture coordinates need to be in [0,1]*[0,1]
    float2 texCoord = 0.5f * 
                  (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);
    //allign texels to pixels
    texCoord -= HalfPixel;

    float3 normal;
    float2 specularParams;
    LookUpNormalAndSpecularParams(texCoord, normal, specularParams);

    //compute world space position
    float4 position;
    //read depth
    float depthVal = tex2D(DepthSampler, texCoord).r;
    //compute screen-space position
    position.xy = input.ScreenPosition.xy;
    position.z = depthVal;
    position.w = 1.0f;
    //transform to world space
    position = mul(position, InverseViewProjection);
    position /= position.w; 

    //surface-to-light vector
    float3 lightVector = LightPosition - position;
    //compute attenuation based on distance - linear attenuation
    float ratio = length(lightVector) / LightRadius;
    float attenuation = saturate(1.0f - (ratio * ratio));
    //normalize light vector
    lightVector = normalize(lightVector);
    //compute diffuse light
    float3 diffuseLight;
    ComputeDiffuseLight(normal, lightVector, diffuseLight);

    lightVector = -lightVector;
    //reflection vector
    float3 reflectionVector = normalize(reflect(lightVector, normal));
    //compute specular
    float specularLight = ComputeSpecularLight(position, reflectionVector, specularParams);
    
    //compute spot light factor
    float cosAngle = dot(lightVector, LightDirection);
    float spotFactor = smoothstep(SpotLightCosOuterCone, SpotLightCosInnerCone, cosAngle);

    float shadowFactor = 1.0f;
    if(spotFactor > 0.1f)
    {
        float4 shadowMapUV = mul(position, ShadowMapWarpMatrix);
        float distToCurPixel = shadowMapUV.z / shadowMapUV.w;
        
        shadowFactor = 0.0f;
        //do 9 lookups, but count the middle one twice to make a round number
        shadowFactor += OffsetLookup(ShadowMapSampler, shadowMapUV + float4(0.0f, 0.0f, 0.0f, 0.0f)) > distToCurPixel ? 0.2f : 0.05f;
        shadowFactor += OffsetLookup(ShadowMapSampler, shadowMapUV + float4(1.0f, 0.0f, 0.0f, 0.0f)) > distToCurPixel ? 0.1f : 0.025f;
        shadowFactor += OffsetLookup(ShadowMapSampler, shadowMapUV + float4(0.0f, 1.0f, 0.0f, 0.0f)) > distToCurPixel ? 0.1f : 0.025f;
        shadowFactor += OffsetLookup(ShadowMapSampler, shadowMapUV + float4(-1.0f, 0.0f, 0.0f, 0.0f)) > distToCurPixel ? 0.1f : 0.025f;
        shadowFactor += OffsetLookup(ShadowMapSampler, shadowMapUV + float4(0.0f, -1.0f, 0.0f, 0.0f)) > distToCurPixel ? 0.1f : 0.025f;
        shadowFactor += OffsetLookup(ShadowMapSampler, shadowMapUV + float4(1.0f, 1.0f, 0.0f, 0.0f)) > distToCurPixel ? 0.1f : 0.025f;
        shadowFactor += OffsetLookup(ShadowMapSampler, shadowMapUV + float4(-1.0f, 1.0f, 0.0f, 0.0f)) > distToCurPixel ? 0.1f : 0.025f;
        shadowFactor += OffsetLookup(ShadowMapSampler, shadowMapUV + float4(1.0f, -1.0f, 0.0f, 0.0f)) > distToCurPixel ? 0.1f : 0.025f;
        shadowFactor += OffsetLookup(ShadowMapSampler, shadowMapUV + float4(-1.0f, -1.0f, 0.0f, 0.0f)) > distToCurPixel ? 0.1f : 0.025f;
    }

    //take into account attenuation and LightIntensity.
    //return float4(input.ScreenPosition.xy, 0.0f, 1.0f);
    return attenuation * spotFactor * shadowFactor * LightIntensity * float4(diffuseLight.rgb, specularLight);
}

technique SpotLightTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 ShapedLightVertexShader();
        PixelShader = compile ps_3_0 SpotLightPixelShader();
    }
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
    float4x4 WorldViewProjection = mul(mul(World, View), Projection);
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

technique ShadowMapTechnique
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 ShadowMapVertexShader();
        PixelShader = compile ps_2_0 ShadowMapPixelShader();
    }
}

texture LightMap;

sampler LightSampler = sampler_state
{    
    Texture = (LightMap);    
    AddressU = CLAMP;    
    AddressV = CLAMP;    
    MagFilter = LINEAR;    
    MinFilter = LINEAR;    
    Mipfilter = LINEAR;
};

float4 CombineLightWithColorPixelShader(DirectionalLightShaderOutput input) : COLOR0
{
    float3 diffuseColor = tex2D(ColorSampler, input.TexCoord).rgb;
    float4 light = tex2D(LightSampler, input.TexCoord);
    float3 diffuseLight = light.rgb;
    float specularLight = light.a;

    return float4((diffuseColor * diffuseLight + specularLight), 1);
}

technique CombineLightWithColorTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 ScreenAlignedQuadVertexShader();
        PixelShader = compile ps_2_0 CombineLightWithColorPixelShader();
    }
}
