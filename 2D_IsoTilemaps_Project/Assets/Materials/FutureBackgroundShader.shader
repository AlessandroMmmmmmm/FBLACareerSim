Shader "Custom/FutureBackground"
{
    Properties
    {
        _TopColor    ("Top Color",    Color) = (0.30, 0.65, 1.00, 1)   // bright sky blue
        _MidColor    ("Mid Color",   Color) = (0.75, 0.92, 1.00, 1)    // pale airy cyan
        _HorizonColor("Horizon Color",Color) = (1.00, 0.96, 0.80, 1)  // warm cream white
        _GlowColor   ("Glow Color",  Color) = (1.00, 0.88, 0.45, 1)   // sunny yellow glow
        _GlowHeight  ("Glow Height", Range(0,0.5)) = 0.18
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f    { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            fixed4 _TopColor, _MidColor, _HorizonColor, _GlowColor;
            float  _GlowHeight;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                float y = i.uv.y; // 0=bottom, 1=top

                // Base gradient: top → mid → horizon
                fixed4 col = lerp(_HorizonColor, _MidColor, smoothstep(0.0, 0.45, y));
                col = lerp(col, _TopColor, smoothstep(0.3, 1.0, y));

                // Warm glow bloom near horizon
                float glow = exp(-abs(y - 0.0) / max(_GlowHeight, 0.001));
                col = lerp(col, _GlowColor, glow * 0.6);

                return col;
            }
            ENDCG
        }
    }
}