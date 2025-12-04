// Show a fullscreen texture. Use the default unity Quad mesh!
Shader "Orchid Seal/CameraOverride/Camera Override"
{
    Properties
    {
        _CameraLeftEyeTexture("Camera Left Eye", 2D) = "black" {}
        _CameraRightEyeTexture("Camera Right Eye", 2D) = "black" {}
    }
    SubShader
    {
        Tags
        {
            "DisableBatching" = "true"
            "ForceNoShadowCasting" = "True"
            "IgnoreProjector" = "True"
            "LightMode" = "Always"
            "Queue" = "Overlay"
            "RenderType" = "Overlay"
        }
        
        Cull Off
        Lighting Off
        ZTest Always
        ZWrite Off

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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _CameraLeftEyeTexture;
            float4 _CameraLeftEyeTexture_ST;
            
            sampler2D _CameraRightEyeTexture;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                float2 uv = TRANSFORM_TEX(v.uv, _CameraLeftEyeTexture);
                o.vertex = float4(float2(1.0, -1.0) * (uv * 2.0 - 1.0), 0.0, 1.0);
                o.uv = uv;
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                half4 color = (unity_StereoEyeIndex == 0) ? tex2D(_CameraLeftEyeTexture, i.uv) : tex2D(_CameraRightEyeTexture, i.uv);
                // You could do custom post-processing here. Although, most post processing effects
                // would be better with a grab pass because that wouldn't require redrawing objects.
                // But some post processing uses past frames or extra fullscreen textures like
                // normals or material IDs. And that would be possible here!
                return color;
            }
            ENDCG
        }
    }
}
