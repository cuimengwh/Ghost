Shader "Custom/SkyboxShader"
{
    Properties
    {
        _SunPosition ("Sun Position", Vector) = (0, 0.5, 0, 0)
        _SunSize ("Sun Size", Range(0.001, 0.1)) = 0.05
        _SunColor ("Sun Color", Color) = (1, 0.9, 0.8, 1)
        _SunIntensity ("Sun Intensity", Range(0, 10)) = 1.0
        
        _SkyColorTop ("Sky Color Top", Color) = (0.05, 0.15, 0.3, 1)
        _SkyColorBottom ("Sky Color Bottom", Color) = (0.3, 0.6, 0.9, 1)
        _HorizonColor ("Horizon Color", Color) = (0.8, 0.9, 1.0, 1)
        _HorizonSize ("Horizon Size", Range(0.01, 0.5)) = 0.1
        
        _StarsIntensity ("Stars Intensity", Range(0, 1)) = 0.5
        _StarsTexture ("Stars Texture", 2D) = "black" {}
        _StarsTiling ("Stars Tiling", Range(0.1, 10)) = 1.0
        
        _CloudsIntensity ("Clouds Intensity", Range(0, 1)) = 0.3
        _CloudsTexture ("Clouds Texture", 2D) = "white" {}
        _CloudsSpeed ("Clouds Speed", Range(0, 0.1)) = 0.01
        _CloudsTiling ("Clouds Tiling", Range(0.1, 10)) = 2.0
        
        _TimeOfDay ("Time of Day", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
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
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };
            
            // 属性变量
            float3 _SunPosition;
            float _SunSize;
            float4 _SunColor;
            float _SunIntensity;
            
            float4 _SkyColorTop;
            float4 _SkyColorBottom;
            float4 _HorizonColor;
            float _HorizonSize;
            
            float _StarsIntensity;
            sampler2D _StarsTexture;
            float _StarsTiling;
            
            float _CloudsIntensity;
            sampler2D _CloudsTexture;
            float _CloudsSpeed;
            float _CloudsTiling;
            
            float _TimeOfDay;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                // 计算世界位置
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // 计算视图方向（从顶点到相机）
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // 使用视图方向
                float3 viewDir = normalize(i.viewDir);
                
                // 计算天空渐变
                float horizon = 1.0 - abs(viewDir.y);
                float4 skyColor = lerp(_SkyColorBottom, _SkyColorTop, viewDir.y);
                float4 horizonColor = _HorizonColor * smoothstep(0.0, _HorizonSize, horizon);
                skyColor = lerp(skyColor, horizonColor, smoothstep(0.0, _HorizonSize, horizon));
                
                // 计算太阳 - 使用更精确的方法
                float3 sunDir = normalize(_SunPosition);
                float sunDot = dot(viewDir, sunDir);
                
                // 太阳核心
                float sunCore = smoothstep(1.0 - _SunSize, 1.0, sunDot) * _SunIntensity;
                
                // 太阳光晕
                float sunHalo = smoothstep(1.0 - _SunSize * 3.0, 1.0, sunDot) * _SunIntensity * 0.5;
                
                // 组合太阳效果
                float4 sunColor = _SunColor * (sunCore + sunHalo);
                
                // 确保太阳不会变成黑色
                sunColor = max(sunColor, float4(0.1, 0.1, 0.1, 1.0) * _SunIntensity);
                
                // 计算星星 (夜晚)
                float stars = 0.0;
                if (_TimeOfDay < 0.25 || _TimeOfDay > 0.75)
                {
                    // 使用世界位置计算星星UV，避免依赖纹理坐标
                    float2 starsUV = i.worldPos.xz * _StarsTiling * 0.01;
                    stars = tex2D(_StarsTexture, starsUV).r * _StarsIntensity;
                    
                    // 添加闪烁效果
                    float flicker = sin(_Time.y * 5.0 + i.worldPos.x * 50.0) * 0.5 + 0.5;
                    stars *= flicker * 0.3 + 0.7;
                }
                
                // 计算云层
                float2 cloudsUV = i.worldPos.xz * _CloudsTiling * 0.01 + float2(_Time.y * _CloudsSpeed, 0);
                float clouds = tex2D(_CloudsTexture, cloudsUV).r * _CloudsIntensity;
                
                // 混合所有效果
                float4 finalColor = skyColor + sunColor;
                finalColor.rgb += stars;
                finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb * (1.0 - clouds), clouds);
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    Fallback "Skybox/Procedural"
}