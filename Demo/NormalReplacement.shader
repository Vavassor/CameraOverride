Shader "OrchidSeal/CameraOverride/Normal Replacement"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float3 normalWs : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                
                // Wobble vertices
                float3 positionOs = v.vertex + 0.03 * sin(10 * v.vertex.xyz + 2 * _Time.y);
                
                o.vertex = UnityObjectToClipPos(positionOs);
                o.normalWs = mul(unity_ObjectToWorld, v.normal);
                UNITY_TRANSFER_FOG(o,o.vertex);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Visualize world-space normals.
                half4 col = half4(0.5 * i.normalWs + 0.5, 1);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
