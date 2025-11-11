Shader "Custom/LaserBeam"
{
    Properties
    {
        _Color ("Laser Color", Color) = (1, 0, 0, 1)
        _Intensity ("Intensity", Range(0, 10)) = 2
        _FresnelPower ("Fresnel Power", Range(0.1, 5)) = 2
        _ScrollSpeed ("Scroll Speed", Float) = 1
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
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };
            
            float4 _Color;
            float _Intensity;
            float _FresnelPower;
            float _ScrollSpeed;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Fresnel effect for glow around edges
                float fresnel = pow(1.0 - saturate(dot(i.viewDir, i.normal)), _FresnelPower);
                
                // Scrolling pattern (negative to flow outward/up)
                float scroll = frac(i.uv.y - _Time.y * _ScrollSpeed);
                float pattern = sin(scroll * 3.14159 * 4) * 0.5 + 0.5;
                
                // Center glow (brighter in the middle)
                float centerGlow = 1.0 - abs(i.uv.x * 2.0 - 1.0);
                centerGlow = pow(centerGlow, 2);
                
                // Combine effects
                float glow = (fresnel + centerGlow) * pattern;
                
                float4 finalColor = _Color * _Intensity * glow;
                finalColor.a = saturate(glow);
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}

