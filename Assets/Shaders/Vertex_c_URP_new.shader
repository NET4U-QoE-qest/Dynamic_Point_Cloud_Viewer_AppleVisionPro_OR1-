Shader "Custom/Vertex_c_URP_new"
{
    Properties
    {
        _PointSize ("Point Size", Float) = 0.01
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _PointSize;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
                float pointSize : PSIZE; // <-- Add this line
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.color = IN.color;
                OUT.pointSize = _PointSize; // <-- Set point size here
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                return IN.color;
            }
            ENDHLSL
        }
    }
}
