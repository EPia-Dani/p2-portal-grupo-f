Shader "Custom/LaserBeam"
{
    Properties
    {
        _CoreColor ("Core Color", Color) = (1, 0, 0, 1)
        _OutlineColor ("Outline Color", Color) = (1, 1, 0, 1)
        _BaseColor ("Base Color", Color) = (0.2, 0, 0, 1)
        _Intensity ("Intensity", Range(0, 10)) = 2
        _CoreSize ("Core Size", Range(0.0, 1.0)) = 0.3
        _OutlineSize ("Outline Size", Range(0.0, 1.0)) = 0.7
        _ScrollSpeed ("Scroll Speed", Float) = 1.0
        _BandFrequency ("Band Frequency", Float) = 3.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            float4 _CoreColor;
            float4 _OutlineColor;
            float4 _BaseColor;
            float _Intensity;
            float _CoreSize;
            float _OutlineSize;
            float _ScrollSpeed;
            float _BandFrequency;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Distance from center across X (0 at center, 1 at edges)
                float dist = abs(i.uv.x * 2.0 - 1.0);

                // Masks
                float core = 1.0 - smoothstep(_CoreSize * 0.98, _CoreSize, dist);
                float outline = 1.0 - smoothstep(_OutlineSize * 0.98, _OutlineSize, dist);
                outline = saturate(outline - core); // only the ring outside the core

                // Compose colors: base -> outline -> core
                float4 color = _BaseColor;
                color = lerp(color, _OutlineColor, outline);
                color = lerp(color, _CoreColor, core);

                // Subtle animated brightness band along length
                float anim = 0.5 + 0.5 * sin(6.2831853 * (i.uv.y * _BandFrequency - _Time.y * _ScrollSpeed));
                float brightness = 0.85 + 0.15 * anim;

                color *= (_Intensity * brightness);
                color.a = 1.0;
                return color;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}

