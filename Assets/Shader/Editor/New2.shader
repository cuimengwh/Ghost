Shader "Custom/Character/CompleteCharacter3" 
{
    Properties 
    {
        // 主颜色和纹理
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base Texture", 2D) = "white" {}
        
        // 卡通着色属性
        _RampTex ("Ramp Texture", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (0.5,0.5,0.5,1)
        _ShadowThreshold ("Shadow Threshold", Range(0,1)) = 0.5
        _ShadowSmoothness ("Shadow Smoothness", Range(0,1)) = 0.1
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0,0.1)) = 0.01
        
        // Alpha和透明属性
        _AlphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _Transparency ("Transparency", Range(0,1)) = 1.0
        
        // 渲染设置
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2
        [Enum(Off,0,On,1)] _ZWrite ("ZWrite", Float) = 1
        
        // 部分标识
        [Toggle] _IsEyesBrows ("Is Eyes/Brows", Float) = 0
    }
    
    SubShader 
    {
        Tags 
        { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True" 
        }
        LOD 200
        
        // 深度预通道 - 只渲染眉毛的深度信息
        Pass
        {
            Name "DEPTH_PREPASS"
            Tags { "LightMode" = "Always" }
            ColorMask 0  // 不写入颜色，只写入深度
            ZWrite On
            ZTest LEqual
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ EYESBROWS_ON
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _AlphaCutoff;
            float _IsEyesBrows;
            
            struct appdata 
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f 
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            v2f vert (appdata v) 
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target 
            {
                // 只处理眉毛的深度预渲染
                #ifdef EYESBROWS_ON
                fixed4 col = tex2D(_MainTex, i.uv);
                clip(col.a - _AlphaCutoff);
                #endif
                return 0;
            }
            ENDCG
        }
        
        // 轮廓线Pass
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
            Cull Front
            ZWrite On
            
            Stencil {
                Ref 0
                Comp Always
                Pass Keep
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            float _OutlineWidth;
            float4 _OutlineColor;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _AlphaCutoff;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                
                float4 pos = mul(UNITY_MATRIX_MV, v.vertex);
                float3 normal = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                normal.z = -0.5;
                pos = pos + float4(normalize(normal), 0) * _OutlineWidth;
                o.pos = mul(UNITY_MATRIX_P, pos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }
            
            half4 frag(v2f i) : COLOR
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                clip(col.a - _AlphaCutoff);
                return _OutlineColor;
            }
            ENDCG
        }
        
        // 主体Pass
        Pass
        {
            Name "MAIN"
            Tags { "LightMode" = "ForwardBase" }
            Cull [_CullMode]
            ZWrite Off  // 关闭深度写入

            ZTest Always

            ZTest Equal  // 只渲染与预通道深度相等的片段
            Blend SrcAlpha OneMinusSrcAlpha
            
            Stencil {
                Ref 0
                Comp Always
                Pass Keep
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile _ EYESBROWS_ON
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _RampTex;
            float4 _Color;
            float4 _ShadowColor;
            float _ShadowThreshold;
            float _ShadowSmoothness;
            fixed _AlphaCutoff;
            fixed _Transparency;
            
            struct appdata 
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f 
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                LIGHTING_COORDS(3,4)
            };
            
            v2f vert (appdata v) 
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = normalize(UnityObjectToWorldNormal(v.normal));
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target 
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                clip(col.a - _AlphaCutoff);
                
                #ifdef EYESBROWS_ON
                // 眼睛和眉毛的处理
                col.a *= _Transparency;
                #else
                // 角色主体和头发的处理
                // 卡通着色计算
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = dot(i.normal, lightDir);
                float attenuation = LIGHT_ATTENUATION(i);
                
                float ramp = tex2D(_RampTex, float2(NdotL * 0.5 + 0.5, 0.5)).r;
                float shadow = smoothstep(_ShadowThreshold - _ShadowSmoothness, 
                                         _ShadowThreshold + _ShadowSmoothness, 
                                         ramp * attenuation);
                
                col.rgb *= lerp(_ShadowColor, _LightColor0, shadow);
                col.rgb += UNITY_LIGHTMODEL_AMBIENT.rgb * col.rgb;
                #endif
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Cutout/Diffuse"
}