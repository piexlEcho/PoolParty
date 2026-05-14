Shader "Custom/ImpactLines"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Effect Intensity", Range(0, 1)) = 0.0
        _LineSpeed ("Line Speed", Float) = 2.0
        _LineDensity ("Line Density", Float) = 80.0
        _LineThreshold ("Line Threshold", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Intensity;
            float _LineSpeed;
            float _LineDensity;
            float _LineThreshold;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f    { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            //

            float hash(float n) { return frac(sin(n) * 43758.5453); }

            // speed lines — arctangent2 + noise chain
            float speedLines(float2 uv, float time)
            {
                float2 centered = uv - 0.5;
                float angle = atan2(centered.y, centered.x);
                float dist  = length(centered);

                // noise - floor
                float t     = floor(time * _LineSpeed);
                float noise = hash(floor(angle * _LineDensity) + t * 127.1);

                // radial fade - scale 
                float lines = step(_LineThreshold, noise) * smoothstep(0.0, 0.35, dist);
                return lines;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // desat (b&w)
                float grey   = dot(col.rgb, float3(0.299, 0.587, 0.114));
                fixed4 bwCol = fixed4(grey, grey, grey, col.a);

                // speed lines
                float lines  = speedLines(i.uv, _Time.y);
                fixed4 final = lerp(bwCol, fixed4(1,1,1,1), lines * _Intensity);

                // blend
                return lerp(col, final, _Intensity);
            }
            ENDCG
        }
    }
}