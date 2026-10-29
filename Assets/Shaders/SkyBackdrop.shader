Shader "Sky/Backdrop"
{
    Properties
    {
        _MainTex ("Dusk", 2D) = "white" {}
        _SecondTex ("Night", 2D) = "white" {}
        _Blend ("Blend", Range(0, 1)) = 0
        _Tint ("Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Opaque" "PreviewType" = "Plane" }
        Cull Off
        ZWrite Off
        ZTest Always

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

            half4 _Tint;
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
                half3 dusk = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).rgb;
                half3 night = SAMPLE_TEXTURE2D(_SecondTex, sampler_SecondTex, IN.uv).rgb;
                half3 color = lerp(dusk, night, saturate(_Blend));
                return half4(color * _Tint.rgb, 1);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
