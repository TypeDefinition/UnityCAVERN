Shader "Spelunx/CavernProjection" {
    // Properties are options set per material, exposed by the material inspector.
    // By convention, properties' names are declared _PropertyName("Label In Inspector", Type) = Value.
    // List of property types: https://docs.unity3d.com/Manual/SL-Properties.html
    Properties {
        [Header(Projection Options)]
        _CubemapMono ("Cubemap Mono", Cube) = "" {}
        _CubemapLeft ("Cubemap Left", Cube) = "" {}
        _CubemapRight ("Cubemap Right", Cube) = "" {}
        _CubemapFront ("Cubemap Front", Cube) = "" {}
        _CubemapBack ("Cubemap Back", Cube) = "" {}
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