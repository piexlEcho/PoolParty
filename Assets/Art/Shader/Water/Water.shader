Shader "Custom/Water_Advanced_Interactive"
{
    Properties
    {
        [Header(Color)]
        _NearColor ("Near Color", Color) = (0.2, 0.6, 0.9, 1)
        _FarColor ("Far Color", Color) = (0.0, 0.3, 0.6, 1)

        [Header(Alpha)]
        _NearAlpha ("Near Alpha", Range(0, 1)) = 1
        _FarAlpha ("Far Alpha", Range(0, 1)) = 0.5

        [Header(Distance)]
        _MinDistance ("Min Distance", Float) = 0
        _MaxDistance ("Max Distance", Float) = 20

        [Header(Noise)]
        _NoiseTex ("Noise", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 1
        _NoiseStrength ("Noise Strength", Float) = 2

        [Header(Distortion)]
        _DistortTex ("Distort", 2D) = "gray" {}
        _DistortScale ("Distort Scale", Float) = 1
        _DistortStrength ("Distort Strength", Float) = 0.08

        [Header(Surface)]
        _MainTex ("Main Tex", 2D) = "white" {}
        _MainTexScale ("Scale", Float) = 0.3
        _MainTexSpeed ("Speed", Vector) = (0.02, 0.01, 0, 0)

        [Header(MainTex Intensity)]
        _MainTexIntensity ("Global Intensity", Range(0, 1)) = 1
        _MainTexNear ("Near Intensity", Range(0, 1)) = 1
        _MainTexFar ("Far Intensity", Range(0, 1)) = 0.3

        [Header(Caustics)]
        _CausticsTex ("Caustics", 2D) = "white" {}
        _CausticsScale ("Scale", Float) = 1.2
        _CausticsSpeed ("Speed", Vector) = (0.15, -0.1, 0, 0)
        _CausticsIntensity ("Intensity", Range(0, 2)) = 1
        _CausticsDepth ("Depth Range", Float) = 1.5

        [Header(Rainbow)]
        _RainbowStrength ("Strength", Range(0, 2)) = 0.8
        _RainbowScale ("Frequency", Float) = 8
        _RainbowSpeed ("Speed", Float) = 1.2

        [Header(Edge)]
        _EdgeWidth ("Width", Float) = 0.2
        _EdgeColor ("Color", Color) = (1, 1, 1, 1)
        _EdgeIntensity ("Intensity", Range(0, 1)) = 0.8

        // ===== 交互涟漪 =====
        [Header(Interactive Ripple)]
        _RippleDistortStrength ("Ripple Distort", Float) = 0.15 // 涟漪对UV的扰动强度

        // ===== 遮罩颜色 =====
        [Header(Ripple Mask)]
        _RippleColor ("Mask Color", Color) = (0.3, 0.8, 1, 1)
        _RippleColorStrength ("Color Strength", Range(0, 10)) = 1
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Lambert alpha
        #pragma target 3.0

        sampler2D _CameraDepthTexture;

        // ===== RT输入 =====
        sampler2D _GlobalEffectRT;
        float _OrthographicCamSize;
        float3 _Position;

        struct Input
        {
            float3 worldPos;
            float3 viewDir;
            float4 screenPos;
        };

        fixed4 _NearColor, _FarColor;
        float _NearAlpha, _FarAlpha;
        float _MinDistance, _MaxDistance;

        sampler2D _NoiseTex;
        float _NoiseScale, _NoiseStrength;

        sampler2D _DistortTex;
        float _DistortScale, _DistortStrength;

        sampler2D _MainTex;
        float _MainTexScale;
        float4 _MainTexSpeed;

        float _MainTexIntensity, _MainTexNear, _MainTexFar;

        sampler2D _CausticsTex;
        float _CausticsScale;
        float4 _CausticsSpeed;
        float _CausticsIntensity;
        float _CausticsDepth;

        float _RainbowStrength, _RainbowScale, _RainbowSpeed;

        float _EdgeWidth;
        fixed4 _EdgeColor;
        float _EdgeIntensity;

        float _RippleDistortStrength;
        fixed4 _RippleColor;
        float _RippleColorStrength;

        void surf (Input IN, inout SurfaceOutput o)
        {
            float time = _Time.y;

            // ===== 距离 =====
            float dist = length(IN.worldPos - _WorldSpaceCameraPos);
            float noise = tex2D(_NoiseTex, IN.worldPos.xz * _NoiseScale).r;
            dist += (noise - 0.5) * _NoiseStrength;
            float t = saturate((dist - _MinDistance) / (_MaxDistance - _MinDistance));

            float3 baseColor = lerp(_NearColor.rgb, _FarColor.rgb, t);
            float baseAlpha = lerp(_NearAlpha, _FarAlpha, t);

            float3 viewDir = normalize(IN.viewDir);
            float facing = saturate(dot(viewDir, float3(0, 1, 0)));

            float surfaceMask = 1 - facing;
            float depthMask = facing;

            // ===== 基础扰动 =====
            float2 distort = tex2D(_DistortTex, IN.worldPos.xz * _DistortScale).rg;
            distort = (distort - 0.5) * 2 * _DistortStrength;

            // ===== 涟漪RT =====
            float2 rippleUV = (IN.worldPos.xz - _Position.xz) / (_OrthographicCamSize * 2);
            rippleUV += 0.5;
            //rippleUV.y = 1 - rippleUV.y;  RT翻转，具体需要考虑是否跟随目标

            float4 rippleTex = tex2D(_GlobalEffectRT, rippleUV);

            float ripple = rippleTex.r; // 扰动
            float mask = rippleTex.b; // 蓝色做遮罩

            // ===== UV扰动叠加 =====
            distort += (ripple - 0.5) * _RippleDistortStrength;

            // ===== 主纹理 =====
            float2 mainUV = IN.worldPos.xz * _MainTexScale + time * _MainTexSpeed.xy + distort;
            float3 mainTex = tex2D(_MainTex, mainUV).rgb;

            float mainTexStrength = lerp(_MainTexNear, _MainTexFar, t);
            mainTexStrength *= _MainTexIntensity;
            mainTexStrength *= surfaceMask;

            baseColor = lerp(baseColor, baseColor * mainTex, mainTexStrength);

            // ===== 焦散 =====
            float2 cauUV = IN.worldPos.xz * _CausticsScale + time * _CausticsSpeed.xy + distort * 1.5;
            float caustics = tex2D(_CausticsTex, cauUV).r;

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float sceneDepth = LinearEyeDepth(tex2D(_CameraDepthTexture, screenUV).r);
            float waterDepth = -mul(UNITY_MATRIX_V, float4(IN.worldPos, 1)).z;

            float depthDiff = max(0, sceneDepth - waterDepth);
            float depthFactor = saturate(depthDiff / _CausticsDepth);

            float phase = caustics * _RainbowScale + time * _RainbowSpeed;

            float3 rainbow;
            rainbow.r = sin(phase);
            rainbow.g = sin(phase + 2.094);
            rainbow.b = sin(phase + 4.188);
            rainbow = rainbow * 0.5 + 0.5;

            float3 causticsColor = lerp(caustics.xxx, rainbow, _RainbowStrength * caustics);

            o.Albedo = baseColor;
            o.Alpha = baseAlpha;

            o.Emission = causticsColor * _CausticsIntensity * depthMask * depthFactor;

            // ===== 遮罩上色 =====
            if (mask > 0.01)
            {
                o.Albedo = lerp(o.Albedo, _RippleColor.rgb, mask * _RippleColorStrength);
                o.Alpha = 1; // 完全不透明
            }

            // ===== 边缘 =====
            float edge = 0;
            if (depthDiff > 0 && depthDiff < _EdgeWidth)
            {
                edge = 1 - saturate(depthDiff / _EdgeWidth);
                edge *= _EdgeIntensity;
            }

            if (edge > 0)
            {
                o.Albedo = lerp(o.Albedo, _EdgeColor.rgb, edge);
                o.Alpha = lerp(o.Alpha, 1, edge * 0.5);
            }
        }
        ENDCG
    }

    FallBack "Transparent/Diffuse"
}