Shader "Universal Render Pipeline/Custom/FinalCelShading"
{
    Properties
    {
        [Header(Base Settings)]
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [NoScaleOffset] _RampTex("Ramp Texture (Clamp)", 2D) = "white" {}
        
        [Header(Shadow Settings)]
        _ShadowColor("Shadow Tint", Color) = (0.5,0.5,0.5,1)
        _ShadowThreshold("Shadow Threshold", Range(0,1)) = 0.5
        _ShadowSmoothness("Shadow Smoothness", Range(0,1)) = 0.05
        
        [Header(Outline Settings)]
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth("Outline Width", Range(0, 0.05)) = 0.01
        
        [Header(Normal Map Settings)]
        [Toggle(_NORMALMAP)] _UseNormalMap("Use Normal Map", Float) = 0
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1.0
        
        [Header(Specular Settings)]
        [Toggle(_SPECULAR)] _UseSpecular("Use Specular", Float) = 0
        [HDR] _SpecularColor("Specular Color", Color) = (1,1,1,1)
        _SpecularThreshold("Specular Threshold", Range(0,1)) = 0.9
        _SpecularSmoothness("Specular Smoothness", Range(0,0.3)) = 0.02
        
        [Header(Rim Light Settings)]
        [Toggle(_RIMLIGHT)] _UseRimLight("Use Rim Light", Float) = 0
        [HDR] _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimPower("Rim Power", Range(0.1, 10)) = 5
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.5
        
        [Header(Emission Settings)]
        [Toggle(_EMISSION)] _UseEmission("Use Emission", Float) = 0
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0,1)
        [NoScaleOffset] _EmissionMap("Emission Map", 2D) = "white" {}
        
        [Header(Advanced Settings)]
        [Toggle(_RECEIVE_SHADOWS)] _ReceiveShadows("Receive Shadows", Float) = 1
        _OcclusionStrength("Occlusion Strength", Range(0, 1)) = 1.0
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
    
    // CBUFFER 用于支持 SRP Batcher
    CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        half4 _BaseColor;
        
        half4 _ShadowColor;
        float _ShadowThreshold;
        float _ShadowSmoothness;
        
        half4 _OutlineColor;
        float _OutlineWidth;
        
        float _BumpScale;
        
        half4 _SpecularColor;
        float _SpecularThreshold;
        float _SpecularSmoothness;
        
        half4 _RimColor;
        float _RimPower;
        float _RimThreshold;
        
        half4 _EmissionColor;
        
        float _OcclusionStrength;
        
        half _UseNormalMap;
        half _UseSpecular;
        half _UseRimLight;
        half _UseEmission;
        half _ReceiveShadows;
    CBUFFER_END

    TEXTURE2D(_BaseMap);        SAMPLER(sampler_BaseMap);
    TEXTURE2D(_RampTex);        SAMPLER(sampler_RampTex);
    TEXTURE2D(_BumpMap);        SAMPLER(sampler_BumpMap);
    TEXTURE2D(_EmissionMap);    SAMPLER(sampler_EmissionMap);
    
    // 自定义表面数据结构（避免与URP内置的SurfaceData冲突）
    struct CelShadingSurfaceData
    {
        half3 albedo;
        half3 normalWS;
        half3 emission;
        half alpha;
        half occlusion;
    };
    
    // 光照输入数据
    struct CelShadingLightingData
    {
        half3 normalWS;
        half3 viewDirWS;
        half3 positionWS;
        half shadowAttenuation;
        half fogFactor;
    };
    
    // 卡通光照计算函数
    half CalculateCelIntensity(half NdotL, half shadowAttenuation, half threshold, half smoothness)
    {
        half litOrShadow = NdotL * shadowAttenuation;
        half delta = fwidth(litOrShadow);
        half antialiasing = smoothness + delta * 2.0;
        return smoothstep(threshold - antialiasing, threshold + antialiasing, litOrShadow);
    }

    half CalculateCelSpecular(half3 normalWS, half3 lightDir, half3 viewDir, half threshold, half smoothness)
    {
        half3 halfDir = normalize(lightDir + viewDir);
        half NdotH = dot(normalWS, halfDir);
        half delta = fwidth(NdotH);
        half antialiasing = smoothness + delta * 2.0;
        return smoothstep(threshold - antialiasing, threshold + antialiasing, NdotH);
    }
    
    half CalculateRimLight(half3 normalWS, half3 viewDirWS, half rimPower, half rimThreshold)
    {
        half fresnel = 1.0 - saturate(dot(viewDirWS, normalWS));
        half rim = pow(fresnel, rimPower);
        half delta = fwidth(rim);
        return smoothstep(rimThreshold - delta, rimThreshold + delta, rim);
    }
    
    // 采样法线贴图
    half3 SampleCelNormal(float2 uv, half scale, half4 tangentWS, half3 normalWS)
    {
        #if defined(_NORMALMAP)
            half4 normalSample = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv);
            half3 normalTS = UnpackNormalScale(normalSample, scale);
            float3 bitangent = tangentWS.w * cross(normalWS.xyz, tangentWS.xyz);
            float3x3 tangentToWorld = float3x3(tangentWS.xyz, bitangent.xyz, normalWS.xyz);
            return normalize(TransformTangentToWorld(normalTS, tangentToWorld));
        #else
            return normalWS;
        #endif
    }
    
    // 采样发射贴图
    half3 SampleCelEmission(float2 uv)
    {
        #if defined(_EMISSION)
            return SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uv).rgb * _EmissionColor.rgb;
        #else
            return half3(0, 0, 0);
        #endif
    }
    ENDHLSL

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
            "Queue" = "Geometry"
        }
        LOD 300

        // ==========================================================
        // Pass 1: Outline (Back Face Extrusion)
        // ==========================================================
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            
            Cull Front
            ZWrite On
            ZTest LEqual
            ColorMask RGB
            
            HLSLPROGRAM
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment
            
            #pragma multi_compile_fog
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float fogFactor : TEXCOORD0;
            };

            Varyings OutlineVertex(Attributes input)
            {
                Varyings output;
                
                // 计算轮廓线宽度
                float3 positionOS = input.positionOS.xyz;
                float3 normalOS = normalize(input.normalOS);
                
                // 应用轮廓线宽度
                positionOS += normalOS * _OutlineWidth;
                
                output.positionCS = TransformObjectToHClip(positionOS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }

            half4 OutlineFragment(Varyings input) : SV_Target
            {
                half4 color = _OutlineColor;
                color.rgb = MixFog(color.rgb, input.fogFactor);
                return color;
            }
            ENDHLSL
        }

        // ==========================================================
        // Pass 2: Main Lighting (Forward Lit)
        // ==========================================================
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual
            ColorMask RGB
            Blend One Zero
            
            HLSLPROGRAM
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            // 阴影和多灯光相关关键字
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile_fog
            
            // 功能开关
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _SPECULAR
            #pragma shader_feature_local _RIMLIGHT
            #pragma shader_feature_local _EMISSION
            #pragma shader_feature_local _RECEIVE_SHADOWS

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                #if defined(_NORMALMAP)
                    float4 tangentOS : TANGENT;
                #endif
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                #if defined(_NORMALMAP)
                    float4 tangentWS : TEXCOORD4;
                #endif
                float fogFactor : TEXCOORD5;
                float4 vertexColor : COLOR;
            };

            Varyings LitPassVertex(Attributes input)
            {
                Varyings output;
                
                // 转换顶点位置
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                
                // 转换法线和切线
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS
                    #if defined(_NORMALMAP)
                    , input.tangentOS
                    #endif
                );

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.vertexColor = input.color;
                
                // 计算阴影坐标
                #if defined(_MAIN_LIGHT_SHADOWS) && defined(_RECEIVE_SHADOWS)
                    output.shadowCoord = GetShadowCoord(vertexInput);
                #else
                    output.shadowCoord = float4(0, 0, 0, 0);
                #endif
                
                #if defined(_NORMALMAP)
                    real sign = input.tangentOS.w * GetOddNegativeScale();
                    output.tangentWS = half4(normalInput.tangentWS.xyz, sign);
                #endif
                
                // 计算雾效因子
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }

            half3 ApplyCelShading(CelShadingSurfaceData surface, CelShadingLightingData lighting, Light light)
            {
                // 计算基础光照
                half NdotL = dot(surface.normalWS, light.direction);
                half rampCoord = saturate(NdotL * 0.5 + 0.5);
                half rampValue = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex, float2(rampCoord, 0.5)).r;
                
                // 计算卡通阴影
                half shadowAttenuation = light.shadowAttenuation * light.distanceAttenuation;
                half lightIntensity = CalculateCelIntensity(rampValue, shadowAttenuation, 
                                                            _ShadowThreshold, _ShadowSmoothness);
                
                // 基础颜色混合
                half3 shadowColor = surface.albedo * _ShadowColor.rgb;
                half3 litColor = surface.albedo * light.color;
                half3 baseColor = lerp(shadowColor, litColor, lightIntensity);
                
                // 高光计算
                #if defined(_SPECULAR)
                    half specIntensity = CalculateCelSpecular(surface.normalWS, light.direction, 
                                                            lighting.viewDirWS, _SpecularThreshold, _SpecularSmoothness);
                    specIntensity *= lightIntensity; // 只在光照区域显示高光
                    baseColor += specIntensity * _SpecularColor.rgb * light.color;
                #endif
                
                // 边缘光计算
                #if defined(_RIMLIGHT)
                    half rimIntensity = CalculateRimLight(surface.normalWS, lighting.viewDirWS, 
                                                         _RimPower, _RimThreshold);
                    // 边缘光在背面更强，且受灯光影响
                    rimIntensity *= saturate(NdotL + 0.3);
                    baseColor += rimIntensity * _RimColor.rgb * light.color;
                #endif
                
                return baseColor;
            }

            half4 LitPassFragment(Varyings input) : SV_Target
            {
                // 初始化表面数据
                CelShadingSurfaceData surface;
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                surface.albedo = baseMap.rgb * _BaseColor.rgb;
                surface.alpha = baseMap.a * _BaseColor.a;
                surface.occlusion = 1.0;
                
                // 采样法线
                surface.normalWS = normalize(input.normalWS);
                #if defined(_NORMALMAP)
                    surface.normalWS = SampleCelNormal(input.uv, _BumpScale, input.tangentWS, surface.normalWS);
                #endif
                
                // 初始化光照数据
                CelShadingLightingData lighting;
                lighting.normalWS = surface.normalWS;
                lighting.viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                lighting.positionWS = input.positionWS;
                lighting.fogFactor = input.fogFactor;
                
                // 获取主光源
                Light mainLight;
                #if defined(_MAIN_LIGHT_SHADOWS) && defined(_RECEIVE_SHADOWS)
                    mainLight = GetMainLight(input.shadowCoord);
                #else
                    mainLight = GetMainLight();
                #endif
                
                // 应用卡通着色
                half3 finalColor = ApplyCelShading(surface, lighting, mainLight);
                
                // 添加环境光照
                half3 ambient = SampleSH(surface.normalWS);
                finalColor += ambient * surface.albedo * 0.3;
                
                // 添加附加光源
                #if defined(_ADDITIONAL_LIGHTS)
                    uint pixelLightCount = GetAdditionalLightsCount();
                    for (uint lightIndex = 0; lightIndex < pixelLightCount; ++lightIndex)
                    {
                        Light light = GetAdditionalLight(lightIndex, input.positionWS, 
                                                        half4(1, 1, 1, 1));
                        half3 addLightColor = ApplyCelShading(surface, lighting, light);
                        // 附加光源强度衰减
                        addLightColor *= light.distanceAttenuation;
                        finalColor += addLightColor;
                    }
                #endif
                
                // 添加发射光
                #if defined(_EMISSION)
                    half3 emission = SampleCelEmission(input.uv);
                    finalColor += emission;
                #endif
                
                // 应用顶点色（可选）
                finalColor *= input.vertexColor.rgb;
                
                // 应用雾效
                finalColor = MixFog(finalColor, lighting.fogFactor);
                
                return half4(finalColor, surface.alpha);
            }
            ENDHLSL
        }

        // ==========================================================
        // Pass 3: Shadow Caster (Project Shadows)
        // ==========================================================
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma multi_compile_vertex _ _CAST_SHADOWS_VISUALIZER
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            float3 _LightDirection;

            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                #if _CAST_SHADOWS_VISUALIZER
                    positionWS = TransformWorldToShadowCoord(positionWS).xyz;
                    return TransformWorldToHClip(positionWS);
                #else
                    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                    
                    #if UNITY_REVERSED_Z
                        positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                    #else
                        positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                    #endif
                    
                    return positionCS;
                #endif
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = GetShadowPositionHClip(input);
                output.uv = input.uv;
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                // Alpha测试（如果有透明贴图）
                #if defined(_ALPHATEST_ON)
                    half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * _BaseColor.a;
                    clip(alpha - _Cutoff);
                #endif
                
                return 0;
            }
            ENDHLSL
        }

        // ==========================================================
        // Pass 4: Depth Only (用于深度纹理)
        // ==========================================================
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_Target
            {
                #if defined(_ALPHATEST_ON)
                    half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * _BaseColor.a;
                    clip(alpha - _Cutoff);
                #endif
                
                return 0;
            }
            ENDHLSL
        }
    }
    
    // 降级着色器
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "Queue" = "Geometry"
        }
        LOD 150
        
        Pass
        {
            Name "SimpleLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float fogFactor : TEXCOORD3;
            };
            
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.uv = input.uv;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half3 albedo = baseMap.rgb * _BaseColor.rgb;
                half alpha = baseMap.a * _BaseColor.a;
                
                Light mainLight = GetMainLight();
                half NdotL = dot(input.normalWS, mainLight.direction);
                half lightIntensity = smoothstep(0.0, 0.1, NdotL);
                
                half3 finalColor = albedo * lerp(_ShadowColor.rgb, mainLight.color, lightIntensity);
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}