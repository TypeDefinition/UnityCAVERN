// Header Guards
#ifndef CAVERN_PROJECTION_HLSL
#define CAVERN_PROJECTION_HLSL

// Pull in URP library functions and our own common functions.
// URP library functions can be found via the Unity Editor in "Packages/Universal RP/Shader Library/".
// The HLSL shader files for the URP are in the Packages/com.unity.render-pipelines.universal/ShaderLibrary/ folder in your project.
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// Textures
TEXTURECUBE(_CubemapMono);
SAMPLER(sampler_CubemapMono);
float4 _CubemapMono_ST;

TEXTURECUBE(_CubemapLeft);
SAMPLER(sampler_CubemapLeft);
float4 _CubemapLeft_ST;

TEXTURECUBE(_CubemapRight);
SAMPLER(sampler_CubemapRight);
float4 _CubemapRight_ST;

TEXTURECUBE(_CubemapFront);
SAMPLER(sampler_CubemapFront);
float4 _CubemapFront_ST;

TEXTURECUBE(_CubemapBack);
SAMPLER(sampler_CubemapBack);
float4 _CubemapBack_ST;

// Other Material Properties
int _EnableStereo;

float _CavernHeight;
float _CavernRadius;
float _CavernAngle;
float _CavernElevation;

// Head Tracking Properties
float3 _HeadPositionInverse; // Vector3
float4x4 _HeadRotationInverse; // Matrix4x4

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

// The fragment function, runs once per pixel on the screen.
// It must have a float4 return type and have the SV_TARGET semantic.
// Values in the Vert2Frag have been interpolated based on each pixel's position.
float4 Fragment(Vert2Frag input) : SV_TARGET {
    // Split the screen into 2 halves, top and bottom.
    // For stereoscopic rendering, the top will render the left eye, the bottom will render the right eye.
    // For monoscopic rendering, both halves will render the same thing.
    bool isLeftEye = 0.5 < input.uv.y;
    float2 uv2d = input.uv;
    // For the left eye, convert the UV's y component from the [0.5, 1] range to the [0, 1] range.
    if (isLeftEye) {
        uv2d.y = (uv2d.y - 0.5) * 2.0;
    }
    // For the right eye, convert the UV's y component from the [0, 0.5] range to the [0, 1] range.
    else {
        uv2d.y *= 2.0;
    }

    // Convert the UV from the [0, 1] range to the [-1, 1] range.
    uv2d = uv2d * 2.0 - float2(1.0, 1.0);
    float screenAngle = uv2d.x * _CavernAngle * 0.5f; // Horizontal Screen Angle (Degrees)
    float screenAngleRad = radians(screenAngle);

    // Take note that angle 0 points down the Z-axis, not the X-axis.
    float3 uvCube = float3(_CavernRadius * sin(screenAngleRad), _CavernHeight * 0.5f * uv2d.y + _CavernElevation, _CavernRadius * cos(screenAngleRad));

    // Now transform the screen by the inverse of the player's head position and rotation.
    uvCube = mul(_HeadRotationInverse, float4(uvCube.x, uvCube.y, uvCube.z, 1.0f)).xyz;
    uvCube += _HeadPositionInverse;

    // Monoscopic
    if (!_EnableStereo) {
        return SAMPLE_TEXTURECUBE(_CubemapMono, sampler_CubemapMono, uvCube);
    }
    
    // Left Eye
    if (isLeftEye) {
        // Physical screen left quadrant.
        if (screenAngle < -45.0f) {
            return SAMPLE_TEXTURECUBE(_CubemapBack, sampler_CubemapBack, uvCube);
        }
        // Physical screen right quadrant.
        if (screenAngle > 45.0f) {
            return SAMPLE_TEXTURECUBE(_CubemapFront, sampler_CubemapFront, uvCube);
        }
        // Physical screen front quadrant.
        return SAMPLE_TEXTURECUBE(_CubemapLeft, sampler_CubemapLeft, uvCube);
    }
    
    // Right Eye
    // Physical screen left quadrant.
    if (screenAngle < -45.0f) {
        return SAMPLE_TEXTURECUBE(_CubemapFront, sampler_CubemapFront, uvCube);
    }
    // Physical screen right quadrant.
    if (screenAngle > 45.0f) {
        return SAMPLE_TEXTURECUBE(_CubemapBack, sampler_CubemapBack, uvCube);
    }
    // Physical screen front quadrant.
    return SAMPLE_TEXTURECUBE(_CubemapRight, sampler_CubemapRight, uvCube);
}

#endif // CAVERN_PROJECTION_HLSL