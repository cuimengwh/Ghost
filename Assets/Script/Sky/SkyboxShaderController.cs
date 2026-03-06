using UnityEngine;
using Utopia.Core.Services;
using Utopia.TimeSystem;

/// <summary>
/// 控制天空盒着色器的参数
/// </summary>
public class SkyboxShaderController : MonoBehaviour
{
    private ITimeManager TimeManager;

    [SerializeField] private Material skyboxMaterial;

    [Header("太阳设置")]
    [SerializeField] private AnimationCurve sunHeightCurve = AnimationCurve.Linear(0, -1, 1, 1);
    [SerializeField] private AnimationCurve sunIntensityCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private Gradient sunColorGradient;

    [Header("天空颜色设置")]
    [SerializeField] private Gradient skyTopColorGradient;
    [SerializeField] private Gradient skyBottomColorGradient;
    [SerializeField] private Gradient horizonColorGradient;

    [Header("星星设置")]
    [SerializeField] private AnimationCurve starsIntensityCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("云层设置")]
    [SerializeField] private AnimationCurve cloudsIntensityCurve = AnimationCurve.Constant(0, 1, 0.3f);

    private void Awake()
    {
        if (ServiceLocatorProvider.Global.Locator.TryGet<ITimeManager>(out var timeManager))
        {
            TimeManager = timeManager;
        }
        else
        {
            Debug.LogError("SkyboxShaderController: 无法获取 ITimeManager 服务，请确保已正确注册。");
        }
    }

    private void Start()
    {
        if (skyboxMaterial == null)
        {
            skyboxMaterial = RenderSettings.skybox;
        }
        SetDefaultValues();
        // 订阅时间变化事件
        TimeManager.OnTick += UpdateSkybox;
    }
    private void Update()
    {
        
    }
    private void OnDestroy()
    {
        if (TimeManager != null)
        {
            TimeManager.OnTick -= UpdateSkybox;
        }
    }

    /// <summary>
    /// 根据时间更新天空盒着色器参数
    /// </summary>
    /// <param name="timeOfDay">一天中的时间（0-1）</param>
    private void UpdateSkybox(CustomDateTime timeOfDay)
    {


        if (skyboxMaterial == null) return;
        skyboxMaterial.SetFloat("_TimeOfDay", TimeManager.TimeOfDay);

        // 使用更准确的天文学计算
        float sunAngle = timeOfDay.time * 360f; // 0-360度
        float sunRadians = sunAngle * Mathf.Deg2Rad;

        // 计算太阳在天空中的位置
        // X: 东(-1)到西(1)
        // Y: 地平线(0)到天顶(1)
        // Z: 南(-1)到北(1) - 在这个简单模型中我们主要使用X和Y

        // 太阳高度从-1(地下)到1(天顶)
        float sunHeight = Mathf.Sin(sunRadians);

        // 太阳东西位置从-1(东)到1(西)
        float sunEastWest = Mathf.Cos(sunRadians);

        // 设置太阳位置 (Y是高度，X是东西方向，Z是南北方向)
        // 在这个简单模型中，我们主要关注X和Y，Z设为0
        Vector3 sunPosition = new Vector3(sunEastWest, sunHeight, 0);
        skyboxMaterial.SetVector("_SunPosition", sunPosition);

        // 设置太阳颜色和强度
        skyboxMaterial.SetColor("_SunColor", sunColorGradient.Evaluate(timeOfDay.time));
        skyboxMaterial.SetFloat("_SunIntensity", sunIntensityCurve.Evaluate(timeOfDay.time));

        // 设置天空颜色
        skyboxMaterial.SetColor("_SkyColorTop", skyTopColorGradient.Evaluate(timeOfDay.time));
        skyboxMaterial.SetColor("_SkyColorBottom", skyBottomColorGradient.Evaluate(timeOfDay.time));
        skyboxMaterial.SetColor("_HorizonColor", horizonColorGradient.Evaluate(timeOfDay.time));

        // 设置星星强度（夜晚更强）
        skyboxMaterial.SetFloat("_StarsIntensity", starsIntensityCurve.Evaluate(timeOfDay.time));

        // 设置云层强度
        skyboxMaterial.SetFloat("_CloudsIntensity", cloudsIntensityCurve.Evaluate(timeOfDay.time));

        // 设置时间参数（用于着色器中的条件判断）
        skyboxMaterial.SetFloat("_TimeOfDay", timeOfDay.time);

        // 动态更新环境光
        UpdateAmbientLight(timeOfDay.time);
    }

    /// <summary>
    /// 根据时间更新环境光
    /// </summary>
    /// <param name="timeOfDay">一天中的时间（0-1）</param>
    private void UpdateAmbientLight(float timeOfDay)
    {
        // 使用天空底部颜色作为环境光
        RenderSettings.ambientLight = skyBottomColorGradient.Evaluate(timeOfDay);

        // 根据时间调整环境光强度
        RenderSettings.ambientIntensity = Mathf.Lerp(0.1f, 1.0f, sunIntensityCurve.Evaluate(timeOfDay));
    }

    /// <summary>
    /// 在编辑器中设置默认值
    /// </summary>
    [ContextMenu("设置默认值")]
    private void SetDefaultValues()
    {
        // 太阳高度曲线（东升西落）
        sunHeightCurve = new AnimationCurve(
            new Keyframe(0.0f, -1.0f),   // 午夜 - 太阳在地下
            new Keyframe(0.25f, 0.0f),   // 日出 - 太阳在地平线
            new Keyframe(0.5f, 1.0f),    // 正午 - 太阳在头顶
            new Keyframe(0.75f, 0.0f),   // 日落 - 太阳在地平线
            new Keyframe(1.0f, -1.0f)    // 午夜 - 太阳在地下
        );

        // 太阳强度曲线
        sunIntensityCurve = new AnimationCurve(
            new Keyframe(0.0f, 0.0f),
            new Keyframe(0.2f, 0.3f),
            new Keyframe(0.5f, 1.0f),
            new Keyframe(0.8f, 0.3f),
            new Keyframe(1.0f, 0.0f)
        );

        // 太阳颜色渐变
        sunColorGradient = new Gradient();
        sunColorGradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1.0f, 0.6f, 0.4f), 0.2f),  // 日出 - 橙色
                new GradientColorKey(new Color(1.0f, 0.95f, 0.9f), 0.5f), // 正午 - 白色
                new GradientColorKey(new Color(1.0f, 0.6f, 0.4f), 0.8f)   // 日落 - 橙色
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f)
            }
        );

        // 天空顶部颜色渐变
        skyTopColorGradient = new Gradient();
        skyTopColorGradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.02f, 0.05f, 0.1f), 0.0f),   // 午夜 - 深蓝
                new GradientColorKey(new Color(0.05f, 0.15f, 0.3f), 0.5f),   // 正午 - 蓝色
                new GradientColorKey(new Color(0.02f, 0.05f, 0.1f), 1.0f)    // 午夜 - 深蓝
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f)
            }
        );

        // 天空底部颜色渐变
        skyBottomColorGradient = new Gradient();
        skyBottomColorGradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 0.0f),     // 午夜 - 深蓝
                new GradientColorKey(new Color(0.3f, 0.6f, 0.9f), 0.5f),     // 正午 - 天蓝色
                new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 1.0f)      // 午夜 - 深蓝
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f)
            }
        );

        // 地平线颜色渐变
        horizonColorGradient = new Gradient();
        horizonColorGradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.2f, 0.2f, 0.3f), 0.0f),     // 午夜 - 深蓝灰
                new GradientColorKey(new Color(0.8f, 0.9f, 1.0f), 0.5f),     // 正午 - 浅蓝白
                new GradientColorKey(new Color(0.2f, 0.2f, 0.3f), 1.0f)      // 午夜 - 深蓝灰
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f)
            }
        );

        // 星星强度曲线（夜晚可见）
        starsIntensityCurve = new AnimationCurve(
            new Keyframe(0.0f, 1.0f),
            new Keyframe(0.2f, 0.0f),
            new Keyframe(0.8f, 0.0f),
            new Keyframe(1.0f, 1.0f)
        );

        // 云层强度曲线（全天可见）
        cloudsIntensityCurve = new AnimationCurve(
            new Keyframe(0.0f, 0.3f),
            new Keyframe(1.0f, 0.3f)
        );
    }
}