Shader "UI/RadialVignette"
{
    Properties
    {
        _Color ("Color", Color) = (1,0,0,1)
        _VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.8
        _VignetteSmoothness ("Vignette Smoothness", Range(0, 2)) = 0.5
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        
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
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };
            
            fixed4 _Color;
            float _VignetteIntensity;
            float _VignetteSmoothness;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate distance from center (0.5, 0.5)
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                
                // Normalize distance (0 at center, ~0.707 at corners)
                float normalizedDist = dist * 2.0;
                
                // Apply smoothness curve
                float alpha = pow(saturate(normalizedDist), 1.0 - _VignetteSmoothness);
                alpha *= _VignetteIntensity;
                
                // Apply color and alpha from Image component for blinking
                fixed4 col = _Color;
                col.a *= alpha * i.color.a;
                
                return col;
            }
            ENDCG
        }
    }
}