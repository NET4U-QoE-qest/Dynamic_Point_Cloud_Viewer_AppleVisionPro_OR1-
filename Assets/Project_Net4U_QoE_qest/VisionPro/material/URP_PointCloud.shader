Shader "Custom/URP_PointCloud"
{
    Properties
    {
        _PointSize("Point Size", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            float _PointSize;
 
            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };
 
            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 col : COLOR;
            };
 
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.col = v.color;
                return o;
            }
 
            fixed4 frag(v2f i) : SV_Target
            {
                return i.col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}