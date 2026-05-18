Shader "Custom/StartupBlur"
{
    Properties
    {
        _MainTex  ("Texture", 2D)          = "white" {}
        _BlurSize ("Blur Size", Float)     = 3.0
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
            float4 _MainTex_TexelSize;
            float _BlurSize;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f    { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 texel = _MainTex_TexelSize.xy * _BlurSize;
                fixed4 col = fixed4(0,0,0,0);

                // 9-tap box blur
                col += tex2D(_MainTex, i.uv + float2(-texel.x, -texel.y));
                col += tex2D(_MainTex, i.uv + float2(      0, -texel.y));
                col += tex2D(_MainTex, i.uv + float2( texel.x, -texel.y));
                col += tex2D(_MainTex, i.uv + float2(-texel.x,       0));
                col += tex2D(_MainTex, i.uv + float2(      0,       0));
                col += tex2D(_MainTex, i.uv + float2( texel.x,       0));
                col += tex2D(_MainTex, i.uv + float2(-texel.x,  texel.y));
                col += tex2D(_MainTex, i.uv + float2(      0,  texel.y));
                col += tex2D(_MainTex, i.uv + float2( texel.x,  texel.y));

                return col / 9.0;
            }
            ENDCG
        }
    }
}