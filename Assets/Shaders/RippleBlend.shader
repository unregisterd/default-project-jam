Shader "FX/RippleBlend"
{
    Properties
    {
        _MainTex ("Warm", 2D) = "white" {}
        _SecondTex ("Cool", 2D) = "white" {}
        _Blend ("Blend", Range(0, 1)) = 0
        _Color ("Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha One

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_SecondTex);
            SAMPLER(sampler_SecondTex);

            half4 _Color;
            float _Blend;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 warm = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 cool = SAMPLE_TEXTURE2D(_SecondTex, sampler_SecondTex, IN.uv);
                half4 tex = lerp(warm, cool, saturate(_Blend));
                half3 rgb = tex.rgb * _Color.rgb * _Color.a;
                return half4(rgb * tex.a, 1);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
