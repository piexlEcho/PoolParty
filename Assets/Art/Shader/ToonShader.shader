Shader "Custom/ToonShader"
{
    Properties
    {
        _MainTex ("Base Color Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1, 1, 1, 1)
        _Darkness ("Shadow Brightness Factor", Range(0, 1)) = 0.5
        _DiffuseThreshold ("Diffuse Threshold", Range(0, 1)) = 0.3
        _DiffuseSmoothness ("Diffuse Transition Smoothness", Range(0, 0.5)) = 0.05
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularSize ("Specular Size", Range(0, 1)) = 0.2
        _SpecularGloss ("Specular Glossiness", Range(1, 100)) = 20
        _RimColor ("Rim Light Color", Color) = (0.5, 0.5, 1, 1)
        _RimThreshold ("Rim Light Threshold", Range(0, 1)) = 0.5 // Controls rim light width (higher = narrower band)
        _RimSmoothness ("Rim Light Transition Smoothness", Range(0, 0.5)) = 0.05 // Controls rim edge hardness
        _RimIntensity ("Rim Light Intensity", Range(0, 5)) = 1
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSmoothness ("Shadow Transition Smoothness", Range(0, 0.5)) = 0.05
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

            // ----------------- Shadow Factor (Hard Edge Style) -----------------
            half shadowLower = max(0, _ShadowThreshold - _ShadowSmoothness);
            half shadowUpper = min(1, _ShadowThreshold + _ShadowSmoothness);

            // 1 = fully lit, 0 = fully shadowed
            half shadowFactor = smoothstep(shadowLower, shadowUpper, atten);

            // ----------------- 1. Two-Tone Lambert Diffuse -----------------
            half diffLower = max(0, _DiffuseThreshold - _DiffuseSmoothness);
            half diffUpper = min(1, _DiffuseThreshold + _DiffuseSmoothness);

            // 0~1 diffuse bright area weight
            half diffFactor = smoothstep(diffLower, diffUpper, NdotL);

            // Lit area color = original color × main light color × 1
            half3 litColor = s.Albedo * _LightColor0.rgb;

            // Shadow area color = original color × main light color × _Darkness
            half3 darkColor = s.Albedo * _LightColor0.rgb * _Darkness;

            // Final diffuse:
            // Lit area affected by shadow factor, dark area always preserved
            half3 diffuseColor =
                litColor * diffFactor * shadowFactor +
                darkColor * (1 - diffFactor);

            // ----------------- 2. Hard Edge Specular -----------------
            half spec = 0;

            // Only visible on lit surfaces and affected by shadow factor
            if (NdotL > 0 && shadowFactor > 0)
            {
                half specIntensity = pow(saturate(NdotH), s.Gloss);
                spec = step(_SpecularSize, specIntensity);

                // Hidden in shadow
                spec *= shadowFactor;
            }

            half3 specularColor =
                spec * _SpecularColor * _LightColor0.rgb;

            // ----------------- 3. Hard Edge Rim Light -----------------
            half rim = 0;

            // Only visible on lit surfaces and affected by shadow factor
            if (NdotL > 0 && shadowFactor > 0)
            {
                // Rim intensity based on angle between view direction and normal
                // Larger value = closer to edge
                half rimValue = 1 - saturate(NdotV);

                // Use smoothstep for hard-edge binary effect
                half rimLower = max(0, _RimThreshold - _RimSmoothness);
                half rimUpper = min(1, _RimThreshold + _RimSmoothness);

                rim = smoothstep(rimLower, rimUpper, rimValue);

                rim = rim * _RimIntensity * shadowFactor;
            }

            half3 rimColor = rim * _RimColor;

            // ----------------- 4. Ambient Light -----------------
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