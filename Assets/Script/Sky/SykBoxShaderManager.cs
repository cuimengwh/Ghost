using UnityEditor;
using UnityEngine;

public class SkyboxShaderEditor : ShaderGUI
{
    // 重写ShaderGUI的OnGUI方法，用于自定义材质面板
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // 太阳设置标题
        EditorGUILayout.LabelField("Sun Settings", EditorStyles.boldLabel);
        // 显示太阳位置属性
        materialEditor.ShaderProperty(FindProperty("_SunPosition", properties), "Sun Position");
        // 显示太阳大小属性
        materialEditor.ShaderProperty(FindProperty("_SunSize", properties), "Sun Size");
        // 显示太阳颜色属性
        materialEditor.ShaderProperty(FindProperty("_SunColor", properties), "Sun Color");
        // 显示太阳强度属性
        materialEditor.ShaderProperty(FindProperty("_SunIntensity", properties), "Sun Intensity");

        // 添加空行分隔不同设置区域
        EditorGUILayout.Space();

        // 天空颜色设置标题
        EditorGUILayout.LabelField("Sky Colors", EditorStyles.boldLabel);
        // 显示天空顶部颜色属性
        materialEditor.ShaderProperty(FindProperty("_SkyColorTop", properties), "Sky Top Color");
        // 显示天空底部颜色属性
        materialEditor.ShaderProperty(FindProperty("_SkyColorBottom", properties), "Sky Bottom Color");
        // 显示地平线颜色属性
        materialEditor.ShaderProperty(FindProperty("_HorizonColor", properties), "Horizon Color");
        // 显示地平线大小属性
        materialEditor.ShaderProperty(FindProperty("_HorizonSize", properties), "Horizon Size");

        EditorGUILayout.Space();

        // 星星设置标题
        EditorGUILayout.LabelField("Stars Settings", EditorStyles.boldLabel);
        // 显示星星强度属性
        materialEditor.ShaderProperty(FindProperty("_StarsIntensity", properties), "Stars Intensity");
        // 显示星星纹理属性
        materialEditor.ShaderProperty(FindProperty("_StarsTexture", properties), "Stars Texture");
        // 显示星星平铺属性
        materialEditor.ShaderProperty(FindProperty("_StarsTiling", properties), "Stars Tiling");

        EditorGUILayout.Space();

        // 云层设置标题
        EditorGUILayout.LabelField("Clouds Settings", EditorStyles.boldLabel);
        // 显示云层强度属性
        materialEditor.ShaderProperty(FindProperty("_CloudsIntensity", properties), "Clouds Intensity");
        // 显示云层纹理属性
        materialEditor.ShaderProperty(FindProperty("_CloudsTexture", properties), "Clouds Texture");
        // 显示云层速度属性
        materialEditor.ShaderProperty(FindProperty("_CloudsSpeed", properties), "Clouds Speed");
        // 显示云层平铺属性
        materialEditor.ShaderProperty(FindProperty("_CloudsTiling", properties), "Clouds Tiling");

        EditorGUILayout.Space();

        // 时间设置标题
        EditorGUILayout.LabelField("Time Settings", EditorStyles.boldLabel);
        // 显示时间属性
        materialEditor.ShaderProperty(FindProperty("_TimeOfDay", properties), "Time of Day");

        // 添加一个按钮来应用默认值
        if (GUILayout.Button("Apply Default Values"))
        {
            // 调用应用默认值方法
            ApplyDefaultValues(materialEditor, properties);
        }
    }

    // 应用默认值的方法
    private void ApplyDefaultValues(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // 获取当前编辑的材质
        Material material = materialEditor.target as Material;

        // 确保材质不为空
        if (material != null)
        {
            // 设置太阳位置默认值（Vector4类型）
            material.SetVector("_SunPosition", new Vector4(0, 0.5f, 1, 0));
            // 设置太阳大小默认值
            material.SetFloat("_SunSize", 0.05f);
            // 设置太阳颜色默认值（暖黄色）
            material.SetColor("_SunColor", new Color(1, 0.9f, 0.8f, 1));
            // 设置太阳强度默认值
            material.SetFloat("_SunIntensity", 1.0f);

            // 设置天空顶部颜色默认值（深蓝色）
            material.SetColor("_SkyColorTop", new Color(0.05f, 0.15f, 0.3f, 1));
            // 设置天空底部颜色默认值（浅蓝色）
            material.SetColor("_SkyColorBottom", new Color(0.3f, 0.6f, 0.9f, 1));
            // 设置地平线颜色默认值（淡蓝色）
            material.SetColor("_HorizonColor", new Color(0.8f, 0.9f, 1.0f, 1));
            // 设置地平线大小默认值
            material.SetFloat("_HorizonSize", 0.1f);

            // 设置星星强度默认值
            material.SetFloat("_StarsIntensity", 0.5f);
            // 设置星星平铺默认值
            material.SetFloat("_StarsTiling", 1.0f);

            // 设置云层强度默认值
            material.SetFloat("_CloudsIntensity", 0.3f);
            // 设置云层速度默认值
            material.SetFloat("_CloudsSpeed", 0.01f);
            // 设置云层平铺默认值
            material.SetFloat("_CloudsTiling", 2.0f);

            // 设置时间默认值（中午）
            material.SetFloat("_TimeOfDay", 0.5f);
        }
    }
}