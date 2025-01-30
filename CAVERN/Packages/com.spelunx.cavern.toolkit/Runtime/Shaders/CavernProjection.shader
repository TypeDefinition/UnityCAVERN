Shader "Spelunx/CavernProjection" {
    // Properties are options set per material, exposed by the material inspector.
    // By convention, properties' names are declared _PropertyName("Label In Inspector", Type) = Value.
    // List of property types: https://docs.unity3d.com/Manual/SL-Properties.html
    Properties {
        [Header(Projection Options)]
        [MainTexture] _CubemapMonoEye ("Cubemap Mono Eye", Cube) = "" {}
        _CubemapLeftEye ("Cubemap Left Eye", Cube) = "" {}
        _CubemapRightEye ("Cubemap Right Eye", Cube) = "" {}

        _EnableStereo ("Enable Stereo Rendering", Integer) = 0
        
        _CavernHeight ("Cavern Height", Float) = 0
        _CavernRadius ("Cavern Radius", Float) = 0
        _CavernAngle ("Cavern Angle (Degrees)", Float) = 0

        _CameraRight ("Camera Right Vector", Vector) = (1, 0, 0, 0) // X-Axis
        _CameraFront ("Camera Front Vector", Vector) = (0, 0, 1, 0) // Z-Axis
        _CameraUp ("Camera Up Vector", Vector) = (0, 1, 0, 0) // Y-Axis
    }

    SubShader {
        Tags {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"="Opaque"
        }

        Pass {
            Name "Cavern Projection Mapping"

            HLSLPROGRAM // Begin HLSL code.

            // Tell the shader to find the function named "Vertex" in our HLSL code, and use it as the vertex function.
		    #pragma vertex Vertex
		    // Tell the shader to find the function named "Fragment" in our HLSL code, and use it as the fragment function.
		    #pragma fragment Fragment

            // Include our HLSL code. Seperating it out makes the code reusable.
            #include "HLSL/CavernProjectionMapping.hlsl"

            ENDHLSL // End HLSL code.
        }
    }
}