Shader "GodRays/Particle"
{
    Properties
    {
        [MainTexture] _BaseMap ("Texture", 2D) = "white" {}
        [HDR] _Color ("Tint", Color) = (1, 0.92, 0.75, 1)
        _Intensity ("Intensity", Range(0, 8)) = 1.5
        _AlphaPower ("Alpha Contrast", Range(0.1, 6)) = 1
        _FlickerSpeed ("Flicker Speed", Range(0, 10)) = 1.5
        _FlickerAmount ("Flicker Amount", Range(0, 1)) = 0.15
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        Blend [_SrcBlend] [_DstBlend]
        ZWrite Off
        ZTest [_ZTest]
        Cull Off

        Pass
        {
            Name "GodRaysForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _Color;
                half _Intensity;
                half _AlphaPower;
                half _FlickerSpeed;
                half _FlickerAmount;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.color = v.color;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                half alpha = saturate(tex.a * i.color.a);
                alpha = pow(alpha, _AlphaPower);

                half flicker = 1.0 - _FlickerAmount * (0.5 + 0.5 * sin(_Time.y * _FlickerSpeed + i.uv.y * 6.2831));
                half3 rgb = tex.rgb * _Color.rgb * i.color.rgb * _Intensity * flicker;

                return half4(rgb, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
