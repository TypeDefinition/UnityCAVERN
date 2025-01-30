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
float _CavernHeight;
float _CavernRadius;
float _CavernAngle;
float4x4 _CameraRotation;

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
    bool isTop = 0.5 < input.uv.y;
    float2 uv2d = input.uv;
    // For the left eye, convert the UV's y component from the [0.5, 1] range to the [0, 1] range.
    if (isTop) {
        uv2d.y = (uv2d.y - 0.5) * 2.0;
    }
    // For the right eye, convert the UV's y component from the [0, 0.5] range to the [0, 1] range.
    else {
        uv2d.y *= 2.0;
    }

    // Convert the UV from the [0, 1] range to the [-1, 1] range.
    uv2d = uv2d * 2.0 - float2(1.0, 1.0);
    float horizontalAngle = radians(uv2d.x * _CavernAngle * 0.5f);
    float3 eyeToScreen = normalize(float3(_CavernRadius * sin(horizontalAngle), _CavernHeight * 0.5f * uv2d.y, _CavernRadius * cos(horizontalAngle)));
    float3 uvCube = mul(_CameraRotation, float4(eyeToScreen.x, eyeToScreen.y, eyeToScreen.z, 0.0)).xyz;

    if (_EnableStereo) {
        return isTop ? SAMPLE_TEXTURECUBE(_CubemapLeftEye, sampler_CubemapLeftEye, uvCube) : SAMPLE_TEXTURECUBE(_CubemapRightEye, sampler_CubemapRightEye, uvCube);
    }
    return SAMPLE_TEXTURECUBE(_CubemapMonoEye, sampler_CubemapMonoEye, uvCube);
}

#endif // CAVERN_PROJECTION_HLSL