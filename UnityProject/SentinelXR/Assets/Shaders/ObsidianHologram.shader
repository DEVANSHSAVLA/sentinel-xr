Shader "SentinelXR/ObsidianHologram"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.0, 0.85, 1.0, 0.8)
        _RimColor ("Rim Glow Color", Color) = (0.28, 0.48, 1.0, 1.0)
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 2.5
        _ScanlineSpeed ("Scanline Speed", Range(0.0, 10.0)) = 3.0
        _ScanlineCount ("Scanline Frequency", Range(10.0, 200.0)) = 60.0
        _GlowIntensity ("Glow Intensity", Range(0.5, 5.0)) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 200
        Blend SrcAlpha One
        ZWrite Off
        Cull Back

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _RimColor;
                float _RimPower;
                float _ScanlineSpeed;
                float _ScanlineCount;
                float _GlowIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 normalWS = normalize(input.normalWS);

                // Fresnel Rim Glow
                float NdotV = saturate(dot(normalWS, viewDirWS));
                float fresnel = pow(1.0 - NdotV, _RimPower);

                // Animated Scanline Effect
                float scanline = sin(input.positionWS.y * _ScanlineCount + _Time.y * _ScanlineSpeed) * 0.5 + 0.5;

                float3 finalColor = _BaseColor.rgb * scanline + _RimColor.rgb * fresnel * _GlowIntensity;
                float finalAlpha = saturate(_BaseColor.a * (fresnel + scanline * 0.4));

                return float4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}
