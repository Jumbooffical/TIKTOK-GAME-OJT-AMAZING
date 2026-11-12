Shader "Custom/PlantWindSway"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        [MainColor] _Color("Tint", Color) = (1,1,1,1)

        [Header(Wind Sway Settings)]
        _WindSpeed("Wind Speed", Range(0.0, 10.0)) = 2.0
        _WindStrength("Wind Strength (Horizontal)", Range(0.0, 0.3)) = 0.05
        _WindFrequency("Spatial Frequency (Field Ripple)", Range(0.0, 3.0)) = 0.5
        _WindGust("Gust Turbulence", Range(0.0, 1.0)) = 0.2
        _AnchorY("Root Anchor Y (0 = bottom, 1 = top)", Range(0.0, 1.0)) = 0.0
        _SwayPower("Bending Curve Power", Range(1.0, 4.0)) = 2.0

        [Header(Rendering)]
        [MaterialToggle] _ZWrite("ZWrite", Float) = 0

        // Hidden properties for SpriteRenderer compatibility
        [HideInInspector] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

        Pass
        {
            Tags { "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex UnlitWindVertex
            #pragma fragment UnlitWindFragment

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _WindSpeed;
                float _WindStrength;
                float _WindFrequency;
                float _WindGust;
                float _AnchorY;
                float _SwayPower;
            CBUFFER_END

            void ApplyWindSway(inout float3 positionOS, float2 uv)
            {
                // Height factor: 0 at the root/soil, smoothly bending up to 1 at the top tip
                float height = saturate((uv.y - _AnchorY) / max(0.001, (1.0 - _AnchorY)));
                float swayWeight = pow(height, _SwayPower);

                // Use world position so crops across different farm plots ripple out of sync
                float3 worldPos = TransformObjectToWorld(positionOS);
                float phase = worldPos.x * _WindFrequency + worldPos.y * (_WindFrequency * 0.2);

                // Wind wave oscillation (smooth primary wave + subtle gust)
                float time = _Time.y * _WindSpeed;
                float wave = sin(time + phase);
                wave += sin(time * 1.5 + phase * 1.2) * _WindGust;

                // Displace ONLY horizontally on X axis!
                // NOTE: We do NOT modify positionOS.y to prevent circular/elliptical spinning motion.
                positionOS.x += wave * _WindStrength * swayWeight;
            }

            Varyings UnlitWindVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                ApplyWindSway(input.positionOS, input.uv);

                Varyings o = CommonUnlitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 UnlitWindFragment(Varyings input) : SV_Target
            {
                return CommonUnlitFragment(input, input.color);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/2D/Sprite-Unlit-Default"
}
