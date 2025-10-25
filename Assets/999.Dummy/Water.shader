Shader "Custom/2D/SpriteWorldVerticalDissolve"
{
    Properties
    {
        [MainTexture] _MainTex("Texture", 2D) = "white" {}
        _CutoffHeight("World Y Cutoff Height", Float) = 0.0
        _EdgeSoftness("Edge Softness", Range(0,0.5)) = 0.05
        _Color("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "UniversalMaterialType"="Unlit"
        }

        Pass
        {
            Name "Sprite2D"
            Tags { "LightMode" = "Universal2D" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_ST;
            float _CutoffHeight;
            float _EdgeSoftness;
            float4 _Color;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;

                // 월드 Y 기준 절단
                float dissolve = smoothstep(_CutoffHeight - _EdgeSoftness, _CutoffHeight + _EdgeSoftness, IN.positionWS.y);

                texColor.a *= dissolve;
                clip(texColor.a - 0.001);

                return texColor;
            }
            ENDHLSL
        }
    }
}