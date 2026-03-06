Shader "Brush/BrushEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BrushPos("BrushPos", Vector) = (0, 0, 0, 0)
        _BrushColor("Brush Color", Color) = (1, 1, 1, 1)
        _BrushSize("Brush Size", float) = 0.01
        _ScaleX("ScaleX", float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _BrushPos;
            fixed4 _BrushColor;
            float _BrushSize;
            float _ScaleX;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the main texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // Apply X-axis scaling to UV and brush position
                float2 scaledUV = i.uv;
                scaledUV.x *= _ScaleX;
                float2 scaledBrushPos = _BrushPos.xy;
                scaledBrushPos.x *= _ScaleX;

                // Calculate the distance between the scaled UV and scaled brush position
                float distance = length(scaledUV - scaledBrushPos);

                // Check if the distance is within the brush size
                if (distance < _BrushSize)
                {
                    // If within brush size, use brush color
                    col = _BrushColor;
                }

                return col;
            }
            ENDCG
        }
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(1, 1, 1, 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}