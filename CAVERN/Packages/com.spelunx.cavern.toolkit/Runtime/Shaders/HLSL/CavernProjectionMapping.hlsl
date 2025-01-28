// Header Guards
#ifndef CAVERN_PROJECTION_HLSL
#define CAVERN_PROJECTION_HLSL

// Pull in URP library functions and our own common functions.
// URP library functions can be found via the Unity Editor in "Packages/Universal RP/Shader Library/".
// The HLSL shader files for the URP are in the Packages/com.unity.render-pipelines.universal/ShaderLibrary/ folder in your project.
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// Textures
TEXTURECUBE(_CubemapMonoEye);
SAMPLER(sampler_CubemapMonoEye);
float4 _CubemapMonoEye_ST;

TEXTURECUBE(_CubemapLeftEye);
SAMPLER(sampler_CubemapLeftEye);
float4 _CubemapLeftEye_ST;

TEXTURECUBE(_CubemapRightEye);
SAMPLER(sampler_CubemapRightEye);
float4 _CubemapRightEye_ST;

// Other Material Properties
int _EnableStereo;

// This attributes struct receives data about the mesh we are currently rendering.
// Data is automatically placed in the fields according to their semantic.
// List of available semantics: https://docs.unity3d.com/Manual/SL-VertexProgramInputs.html
struct Attributes { // We can name this struct anything we want.
    float3 positionOS : POSITION; // Position in object space.
    float2 uv : TEXCOORD0; // Material texture UVs.
};

// A struct to define the variables we will pass from the vertex function to the fragment function.
struct Vert2Frag { // We can name this struct anything we want.
    // The output variable of the vertex shader must have the semantics SV_POSITION.
    // This value should contain the position in clip space when output from the vertex function.
    // It will be transformed into pixel position of the current fragment on the screen when read from the fragment function.
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0; // By the variable a TEXCOORDN semantic, Unity will automatically interpolate it for each fragment.
};

// The vertex function, runs once per vertex.
Vert2Frag Vertex(Attributes input) {
    // GetVertexPositionInputs is from ShaderVariableFunctions.hlsl in the URP package.
    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS); // Apply the model-view-projection transformations onto our position.

    Vert2Frag output;
    output.positionCS = positionInputs.positionCS; // Set the clip space position.
    output.uv = input.uv;

    return output;
}

// Assume left eye is on top.
float2 ConvertLeftEyeUV(float2 input) {
    return float2(input.x, (input.y - 0.5) * 2.0);
}

// Assume right eye is on the bottom.
float2 ConvertRightEyeUV(float2 input) {
    return float2(input.x, input.y * 2.0);
}

// The fragment function, runs once per pixel on the screen.
// It must have a float4 return type and have the SV_TARGET semantic.
// Values in the Vert2Frag have been interpolated based on each pixel's position.
float4 Fragment(Vert2Frag input) : SV_TARGET {
    float radius = 3.0f;
    float height = 1.0f;
    
    float2 convertedUV = input.uv;
    // Left Eye
    if (_EnableStereo)
    {
        if (input.uv.y > 0.5)
        {
            convertedUV = ConvertLeftEyeUV(input.uv);
        }
    // Right Eye
        else
        {
            convertedUV = ConvertRightEyeUV(input.uv);
        }
    }
    
    float x = convertedUV.x * 2.0f - 1.0f;
    float y = convertedUV.y * 2.0f - 1.0f;
    
    float horizontalAngle = radians(x * 135.0f);
    float xPos = radius * sin(horizontalAngle);
    float zPos = radius * cos(horizontalAngle);
    float yPos = y * height;
    
    // 3D uv
    float3 uv = float3(xPos, yPos, zPos);
    
    // Left Eye
    if (_EnableStereo)
    {
        if (input.uv.y > 0.5)
        {
            return SAMPLE_TEXTURECUBE(_CubemapLeftEye, sampler_CubemapLeftEye, uv);
        }
    // Right Eye
        return SAMPLE_TEXTURECUBE(_CubemapRightEye, sampler_CubemapRightEye, uv);
    }
    return SAMPLE_TEXTURECUBE(_CubemapMonoEye, sampler_CubemapMonoEye, uv);
}

#endif // CAVERN_PROJECTION_HLSL