// Header Guards
#ifndef CAVERN_PROJECTION_HLSL
#define CAVERN_PROJECTION_HLSL

// Pull in URP library functions and our own common functions.
// URP library functions can be found via the Unity Editor in "Packages/Universal RP/Shader Library/".
// The HLSL shader files for the URP are in the Packages/com.unity.render-pipelines.universal/ShaderLibrary/ folder in your project.
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// Material Properties
// Textures are a little more complicated to deal with.
TEXTURECUBE(_MainTex); // TEXTURECUBE is actually a macro, not a type. This is because behind the scenes, HLSL will replace this with whatever texture type the graphics API you're using. (OpenGL, DirectX, Metal, Vulkan, etc.)
SAMPLER(sampler_MainTex); // The sampler MUST be named "sampler_" + "texture name".
float4 _MainTex_ST; // This contains the UV tiling and offset data, and is automatically set by Unity. It MUST be named "texture name" + "_ST". Used in TRANSFORM_TEX to apply UV tiling.

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
    output.uv = TRANSFORM_TEX(input.uv, _MainTex); // Get the UV position after applying offset & tiling.

    return output;
}

// The fragment function, runs once per pixel on the screen.
// It must have a float4 return type and have the SV_TARGET semantic.
// Values in the Vert2Frag have been interpolated based on each pixel's position.
float4 Fragment(Vert2Frag input) : SV_TARGET {
    float radius = 3.0f;
    float height = 1.0f;
    
    float x = input.uv.x * 2.0f - 1.0f;
    float y = input.uv.y * 2.0f - 1.0f;
    
    float horizontalAngle = radians(x * 135.0f);
    float xPos = radius * sin(horizontalAngle);
    float zPos = radius * cos(horizontalAngle);
    float yPos = y * height;
    
    float3 uv = float3(xPos, yPos, zPos);
    return SAMPLE_TEXTURECUBE(_MainTex, sampler_MainTex, uv);
}

#endif // CAVERN_PROJECTION_HLSL