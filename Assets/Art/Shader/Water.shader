Shader "Custom/Water_WithEdgeHighlight"
{
    Properties
    {
        // ==================== 颜色与透明度 ====================
        [Header(Water Colors)]
        _NearColor ("近处颜色", Color) = (0.2, 0.6, 0.9, 1)
        _FarColor ("远处颜色", Color) = (0.0, 0.3, 0.6, 1)

        [Header(Transparency)]
        _NearAlpha ("近处透明度", Range(0, 1)) = 1.0
        _FarAlpha ("远处透明度", Range(0, 1)) = 0.5

        // ==================== 距离渐变 ====================
        [Header(Distance Gradient)]
        _MinDistance ("最近距离", Float) = 0.0
        _MaxDistance ("最远距离", Float) = 20.0

        // ==================== 噪声扰动 ====================
        [Header(Noise Distortion)]
        _NoiseTex ("噪声纹理", 2D) = "white" {}
        _NoiseScale ("噪声缩放", Float) = 1.0
        _NoiseStrength ("噪声强度", Float) = 2.0

        // ==================== 接触边缘高亮 ====================
        [Header(Contact Edge)]
        _EdgeWidth ("边缘宽度 (世界单位)", Float) = 0.2 // 深度差阈值，控制边缘厚度
        _EdgeColor ("边缘颜色", Color) = (1, 1, 1, 1) // 泡沫/高亮颜色
        _EdgeIntensity ("边缘强度", Range(0, 1)) = 0.8 // 混合强度

        // 占位纹理
        _MainTex ("占位纹理", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows addshadow alpha
        #pragma multi_compile __ SHADOWS_SHADOWMASK
        #pragma multi_compile __ LIGHTMAP_SHADOW_MIXING
        #pragma target 3.0

        sampler2D _CameraDepthTexture;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float4 screenPos;
        };

        // 基础属性
        fixed4 _NearColor, _FarColor;
        float _NearAlpha, _FarAlpha;
        float _MinDistance, _MaxDistance;
        sampler2D _NoiseTex;
        float _NoiseScale, _NoiseStrength;

        // 边缘高亮属性
        float _EdgeWidth;
        fixed4 _EdgeColor;
        float _EdgeIntensity;

        void surf (Input IN, inout SurfaceOutput o)
        {
            // ========== 1. 基础距离渐变（含噪声扰动） ==========
            float distance = length(IN.worldPos - _WorldSpaceCameraPos);
            float2 noiseUV = IN.worldPos.xz * _NoiseScale;
            float noise = tex2D(_NoiseTex, noiseUV).r;
            float offset = (noise - 0.5) * _NoiseStrength;
            float disturbedDistance = distance + offset;
            float t = saturate((disturbedDistance - _MinDistance) / (_MaxDistance - _MinDistance));

            // 基础颜色和透明度
            fixed4 baseColor = lerp(_NearColor, _FarColor, t);
            float baseAlpha = lerp(_NearAlpha, _FarAlpha, t);

            // ========== 2. 接触边缘检测 ==========
            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float sceneDepth = LinearEyeDepth(tex2D(_CameraDepthTexture, screenUV).r);
            float3 viewPos = mul(UNITY_MATRIX_V, float4(IN.worldPos, 1.0)).xyz;
            float waterDepth = -viewPos.z;
            float depthDiff = sceneDepth - waterDepth; // 正值表示水面在前

            // 边缘强度：水面在前且深度差在阈值内，强度线性从1衰减到0
            float edgeStrength = 0;
            if (depthDiff > 0 && depthDiff < _EdgeWidth)
            {
                edgeStrength = 1.0 - saturate(depthDiff / _EdgeWidth);
                edgeStrength *= _EdgeIntensity;
            }

            // 混合最终颜色和透明度
            fixed4 finalColor = baseColor;
            float finalAlpha = baseAlpha;
            if (edgeStrength > 0)
            {
                // 边缘区域叠加边缘颜色（叠加模式）
                finalColor = lerp(baseColor, _EdgeColor, edgeStrength);
                // 可选：让边缘区域略微更不透明
                finalAlpha = lerp(baseAlpha, 1.0, edgeStrength * 0.5);
            }

            o.Albedo = finalColor.rgb;
            o.Alpha = finalAlpha;
            o.Emission = 0;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}