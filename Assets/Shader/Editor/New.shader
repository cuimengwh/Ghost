// 定义Shader路径和名称
Shader "Custom/Character/CompleteCharacter2" 
{
    // 属性块 - 在材质面板中显示的参数
    Properties 
    {
        // 主颜色和纹理
        _Color ("Main Color", Color) = (1,1,1,1)          // 主色调，默认白色
        _MainTex ("Base Texture", 2D) = "white" {}        // 基础纹理，默认白色
        
        // 卡通着色属性
        _RampTex ("Ramp Texture", 2D) = "white" {}        // 渐变纹理，用于卡通着色
        _ShadowColor ("Shadow Color", Color) = (0.5,0.5,0.5,1) // 阴影颜色，默认灰色
        _ShadowThreshold ("Shadow Threshold", Range(0,1)) = 0.5 // 阴影阈值，控制明暗分界
        _ShadowSmoothness ("Shadow Smoothness", Range(0,1)) = 0.1 // 阴影平滑度，控制过渡
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)      // 轮廓线颜色，默认黑色
        _OutlineWidth ("Outline Width", Range(0,0.1)) = 0.01    // 轮廓线宽度
        
        // Alpha和透明属性
        _AlphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.5   // Alpha测试阈值，低于此值的像素被丢弃
        _Transparency ("Transparency", Range(0,1)) = 1.0  // 透明度控制，1为不透明，0为完全透明
        
        // 渲染设置
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2 // 剔除模式
        [Enum(Off,0,On,1)] _ZWrite ("ZWrite", Float) = 1  // 深度写入开关
        
        // 部分标识
        [Toggle] _IsEyesBrows ("Is Eyes/Brows", Float) = 0 // 标记是否为眼睛/眉毛部分
    }
    
    // SubShader块 - 包含多个Pass
    SubShader 
    {
        // 标签 - 告诉Unity如何以及何时渲染这个Shader
        Tags 
        { 
            "Queue" = "Transparent"        // 渲染队列设置为透明队列
            "RenderType" = "Transparent"   // 渲染类型为透明
            "IgnoreProjector" = "True"     // 忽略投影器
        }
        LOD 200 // 细节级别，当相机距离超过设定值时使用更简单的Shader
        
        // 深度预通道 - 只渲染眉毛的深度信息
        // 这个Pass的目的是提前写入深度信息，以便后续Pass可以正确进行深度测试
        Pass
        {
            Name "DEPTH_PREPASS" // Pass名称
            Tags { "LightMode" = "Always" } // 光照模式，Always表示总是执行
            ColorMask 0  // 不写入颜色，只写入深度
            ZWrite On    // 开启深度写入
            ZTest LEqual // 深度测试模式：小于等于当前深度值则通过
            Cull Off     // 关闭剔除，渲染所有面
            
            CGPROGRAM
            // 编译指令
            #pragma vertex vert   // 指定顶点着色器函数
            #pragma fragment frag // 指定片段着色器函数
            #pragma multi_compile _ EYESBROWS_ON // 多编译变体，用于启用/禁用眼睛眉毛处理
            
            #include "UnityCG.cginc" // 包含Unity内置CG函数
            
            // 声明属性变量
            sampler2D _MainTex;
            float4 _MainTex_ST;   // 纹理的缩放和偏移
            fixed _AlphaCutoff;
            float _IsEyesBrows;   // 是否处理眼睛眉毛
            
            // 顶点着色器输入结构
            struct appdata 
            {
                float4 vertex : POSITION; // 顶点位置
                float2 uv : TEXCOORD0;    // 纹理坐标
            };
            
            // 顶点着色器输出结构
            struct v2f 
            {
                float4 vertex : SV_POSITION; // 裁剪空间位置
                float2 uv : TEXCOORD0;       // 纹理坐标
            };
            
            // 顶点着色器函数
            v2f vert (appdata v) 
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // 将顶点位置转换到裁剪空间
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);      // 应用纹理缩放和偏移
                return o;
            }
            
            // 片段着色器函数
            fixed4 frag (v2f i) : SV_Target 
            {
                // 只处理眉毛的深度预渲染
                #ifdef EYESBROWS_ON
                fixed4 col = tex2D(_MainTex, i.uv); // 采样纹理
                clip(col.a - _AlphaCutoff);         // Alpha测试，低于阈值的片段被丢弃
                #endif
                return 0; // 返回黑色，但由于ColorMask 0，实际上不写入颜色
            }
            ENDCG
        }
        
        // 轮廓线Pass - 渲染角色轮廓
        Pass
        {
            Name "OUTLINE" // Pass名称
            Tags { "LightMode" = "Always" } // 光照模式，总是执行
            Cull Front     // 剔除正面，只渲染背面（用于轮廓效果）
            ZWrite On      // 开启深度写入
            
            // 模板测试设置
            Stencil {
                Ref 0                  // 参考值0
                Comp Always            // 总是通过模板测试
                Pass Keep              // 通过测试后保持原模板值
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            // 轮廓线属性
            float _OutlineWidth;
            float4 _OutlineColor;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _AlphaCutoff;
            
            // 顶点着色器输入
            struct appdata
            {
                float4 vertex : POSITION; // 顶点位置
                float3 normal : NORMAL;   // 法线向量
                float2 uv : TEXCOORD0;    // 纹理坐标
            };
            
            // 顶点着色器输出
            struct v2f
            {
                float4 pos : SV_POSITION; // 裁剪空间位置
                float2 uv : TEXCOORD0;    // 纹理坐标
            };
            
            // 顶点着色器 - 计算轮廓偏移
            v2f vert(appdata v)
            {
                v2f o;
                
                // 将顶点转换到视图空间
                float4 pos = mul(UNITY_MATRIX_MV, v.vertex);
                // 将法线转换到视图空间
                float3 normal = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                normal.z = -0.5; // 调整法线Z分量，控制轮廓方向
                // 沿法线方向扩展顶点，形成轮廓
                pos = pos + float4(normalize(normal), 0) * _OutlineWidth;
                // 转换到裁剪空间
                o.pos = mul(UNITY_MATRIX_P, pos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); // 应用纹理变换
                
                return o;
            }
            
            // 片段着色器 - 渲染轮廓颜色
            half4 frag(v2f i) : COLOR
            {
                fixed4 col = tex2D(_MainTex, i.uv); // 采样纹理
                clip(col.a - _AlphaCutoff);         // Alpha测试
                return _OutlineColor;               // 返回轮廓颜色
            }
            ENDCG
        }
        
        // 主体Pass - 渲染角色主要部分
        Pass
        {
            Name "MAIN" // Pass名称
            Tags { "LightMode" = "ForwardBase" } // 前向渲染基础光照
            Cull [_CullMode]    // 使用属性中定义的剔除模式
            ZWrite Off          // 关闭深度写入（重要：为了透明效果）
            ZTest Always        // 总是通过深度测试（确保显示在最前）
            Blend SrcAlpha OneMinusSrcAlpha // 标准Alpha混合模式
            
            // 模板测试设置
            Stencil {
                Ref 0       // 参考值0
                Comp Always // 总是通过模板测试
                Pass Keep   // 通过测试后保持原模板值
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase // 为前向渲染编译多个变体
            #pragma multi_compile _ EYESBROWS_ON // 启用/禁用眼睛眉毛处理
            
            #include "UnityCG.cginc"
            #include "AutoLight.cginc" // 包含光照和阴影功能
            #include "Lighting.cginc"  // 包含光照计算函数
            
            // 属性变量
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _RampTex;
            float4 _Color;
            float4 _ShadowColor;
            float _ShadowThreshold;
            float _ShadowSmoothness;
            fixed _AlphaCutoff;
            fixed _Transparency;
            
            // 顶点着色器输入
            struct appdata 
            {
                float4 vertex : POSITION; // 顶点位置
                float3 normal : NORMAL;   // 法线向量
                float2 uv : TEXCOORD0;    // 纹理坐标
            };
            
            // 顶点着色器输出
            struct v2f 
            {
                float4 pos : SV_POSITION;     // 裁剪空间位置
                float2 uv : TEXCOORD0;        // 纹理坐标
                float3 normal : TEXCOORD1;    // 世界空间法线
                float3 viewDir : TEXCOORD2;   // 视图方向
                LIGHTING_COORDS(3,4)          // 光照坐标（宏展开为TEXCOORD3和4）
            };
            
            // 顶点着色器
            v2f vert (appdata v) 
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);      // 转换到裁剪空间
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);        // 应用纹理变换
                o.normal = normalize(UnityObjectToWorldNormal(v.normal)); // 转换法线到世界空间
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));       // 计算视图方向
                TRANSFER_VERTEX_TO_FRAGMENT(o);              // 传递顶点光照数据到片段着色器
                return o;
            }
            
            // 片段着色器
            fixed4 frag (v2f i) : SV_Target 
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color; // 采样纹理并应用主颜色
                
                // 关键修改：先应用透明度调整，再进行Alpha测试
                // 这样确保透明度调整对所有片段都生效，而不仅仅是那些通过Alpha测试的片段
                #ifdef EYESBROWS_ON
                // 眼睛和眉毛的处理 - 先应用透明度
                col.a *= _Transparency; // 应用透明度
                #endif
                
                // 然后进行Alpha测试 - 修改后的顺序
                clip(col.a - _AlphaCutoff); // Alpha测试，低于阈值的片段被丢弃
                
                // 根据是否处理眼睛/眉毛选择不同的渲染路径
                #ifdef EYESBROWS_ON
                // 眼睛和眉毛不需要光照计算，只需返回调整后的颜色
                #else
                // 角色主体和头发的处理 - 应用卡通着色
                // 计算光照
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz); // 光源方向
                float NdotL = dot(i.normal, lightDir);                 // 法线与光源的点积
                float attenuation = LIGHT_ATTENUATION(i);              // 光照衰减
                
                // 使用渐变纹理计算阴影
                float ramp = tex2D(_RampTex, float2(NdotL * 0.5 + 0.5, 0.5)).r;
                float shadow = smoothstep(_ShadowThreshold - _ShadowSmoothness, 
                                         _ShadowThreshold + _ShadowSmoothness, 
                                         ramp * attenuation);
                
                // 应用阴影颜色和光照颜色
                col.rgb *= lerp(_ShadowColor, _LightColor0, shadow);
                // 添加环境光
                col.rgb += UNITY_LIGHTMODEL_AMBIENT.rgb * col.rgb;
                #endif
                
                return col; // 返回最终颜色
            }
            ENDCG
        }
    }
    // 后备Shader - 当当前Shader不被支持时使用
    FallBack "Transparent/Cutout/Diffuse"
}