Shader "Sky/Gradient"
{
    Properties
    {
        _TopColor ("Top", Color) = (0.32, 0.26, 0.45, 1)
        _HorizonColor ("Horizon", Color) = (1, 0.78, 0.62, 1)
        _GroundColor ("Ground", Color) = (0.16, 0.14, 0.22, 1)
        _Exponent ("Exponent", Float) = 1.4
        _GroundSharpness ("Ground Sharpness", Float) = 6
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 direction : TEXCOORD0;
            };

            half4 _TopColor;
            half4 _HorizonColor;
            half4 _GroundColor;
            float _Exponent;
            float _GroundSharpness;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.direction = IN.positionOS.xyz;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 dir = normalize(IN.direction);
                float up = saturate(dir.y);
                float sky = pow(up, _Exponent);
                half3 color = lerp(_HorizonColor.rgb, _TopColor.rgb, sky);
                float down = saturate(-dir.y * _GroundSharpness);
                color = lerp(color, _GroundColor.rgb, down);
                return half4(color, 1);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
