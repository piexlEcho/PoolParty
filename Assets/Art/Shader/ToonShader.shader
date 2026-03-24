Shader "Custom/ToonShader"
{
    Properties
    {
        _MainTex ("基础色贴图", 2D) = "white" {}
        _Color ("颜色叠加", Color) = (1, 1, 1, 1)
        _Darkness ("暗部亮度系数", Range(0, 1)) = 0.5
        _DiffuseThreshold ("漫反射阈值", Range(0, 1)) = 0.3
        _DiffuseSmoothness ("漫反射过渡平滑度", Range(0, 0.5)) = 0.05
        _SpecularColor ("高光颜色", Color) = (1, 1, 1, 1)
        _SpecularSize ("高光大小", Range(0, 1)) = 0.2
        _SpecularGloss ("高光光泽度", Range(1, 100)) = 20
        _RimColor ("边缘光颜色", Color) = (0.5, 0.5, 1, 1)
        _RimThreshold ("边缘光阈值", Range(0, 1)) = 0.5 // 边缘光出现范围（值越大，光带越窄）
        _RimSmoothness ("边缘光过渡平滑度", Range(0, 0.5)) = 0.05 // 边缘光硬边程度
        _RimIntensity ("边缘光强度", Range(0, 5)) = 1
        _ShadowThreshold ("阴影阈值", Range(0, 1)) = 0.5
        _ShadowSmoothness ("阴影过渡平滑度", Range(0, 0.5)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Toon fullforwardshadows addshadow
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        half _Darkness;
        half _DiffuseThreshold;
        half _DiffuseSmoothness;
        fixed3 _SpecularColor;
        half _SpecularSize;
        half _SpecularGloss;
        fixed3 _RimColor;
        half _RimThreshold;
        half _RimSmoothness;
        half _RimIntensity;
        half _ShadowThreshold;
        half _ShadowSmoothness;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            o.Specular = _SpecularColor;
            o.Gloss = _SpecularGloss;
        }

        half4 LightingToon (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            half3 normal = normalize(s.Normal);
            half3 light = normalize(lightDir);
            half3 view = normalize(viewDir);
            half NdotL = dot(normal, light);
            half NdotV = dot(normal, view);
            half3 halfDir = normalize(light + view);
            half NdotH = dot(normal, halfDir);

            // ----------------- 阴影因子（硬边风格） -----------------
            half shadowLower = max(0, _ShadowThreshold - _ShadowSmoothness);
            half shadowUpper = min(1, _ShadowThreshold + _ShadowSmoothness);
            half shadowFactor = smoothstep(shadowLower, shadowUpper, atten); // 1 = 完全受光，0 = 完全阴影

            // ----------------- 1. 兰伯特二分漫反射（支持软硬过渡） -----------------
            half diffLower = max(0, _DiffuseThreshold - _DiffuseSmoothness);
            half diffUpper = min(1, _DiffuseThreshold + _DiffuseSmoothness);
            half diffFactor = smoothstep(diffLower, diffUpper, NdotL); // 0~1 漫反射亮部权重

            // 亮部颜色 = 原始颜色 × 主光颜色 × 1
            half3 litColor = s.Albedo * _LightColor0.rgb;
            // 暗部颜色 = 原始颜色 × 主光颜色 × _Darkness
            half3 darkColor = s.Albedo * _LightColor0.rgb * _Darkness;

            // 最终漫反射：亮部受阴影因子影响（阴影中消失），暗部始终保留
            half3 diffuseColor = litColor * diffFactor * shadowFactor + darkColor * (1 - diffFactor);

            // ----------------- 2. 硬边高光（仅受光面且阴影因子影响） -----------------
            half spec = 0;
            if (NdotL > 0 && shadowFactor > 0)
            {
                half specIntensity = pow(saturate(NdotH), s.Gloss);
                spec = step(_SpecularSize, specIntensity);
                spec *= shadowFactor; // 阴影中消失
            }
            half3 specularColor = spec * _SpecularColor * _LightColor0.rgb;

            // ----------------- 3. 硬边边缘光（仅受光面且阴影因子影响） -----------------
            half rim = 0;
            if (NdotL > 0 && shadowFactor > 0)
            {
                // 边缘光强度基于视线与法线夹角，值越大越边缘
                half rimValue = 1 - saturate(NdotV);
                // 使用 smoothstep 实现硬边二分
                half rimLower = max(0, _RimThreshold - _RimSmoothness);
                half rimUpper = min(1, _RimThreshold + _RimSmoothness);
                rim = smoothstep(rimLower, rimUpper, rimValue);
                rim = rim * _RimIntensity * shadowFactor;
            }
            half3 rimColor = rim * _RimColor;

            // ----------------- 4. 环境光 -----------------
            half3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * s.Albedo;

            half4 c;
            c.rgb = ambient + diffuseColor + specularColor + rimColor;
            c.a = s.Alpha;
            return c;
        }
        ENDCG
    }
    FallBack "Diffuse"
}