using Spine.Unity;
using UnityEngine;

public class TradeUI : MonoBehaviour
{
    [Header("组件获取")]
    public SkeletonGraphic skeletonGraphic;   // 拖拽 SkeletonGraphic 组件
    public Transform characterTransform;      // 角色 Transform（用于计算方向）

    [Header("骨骼设置")]
    public string boneName = "转向";           // 要控制的骨骼名称

    [Header("圆圈移动范围")]
    public float circleRadius = 100f;          // 在 Canvas 局部坐标系中的半径

    [Header("平滑移动")]
    public bool smoothPosition = true;
    public float smoothSpeed = 10f;            // 位置插值速度

    private Spine.Bone targetBone;              // 目标骨骼
    private Vector2 boneOrigin;                  // 骨骼初始局部位置（圆心）
    private Vector2 targetPosition;               // 目标位置（用于平滑）

    void Start()
    {
        if (skeletonGraphic != null && skeletonGraphic.Skeleton != null)
        {
            targetBone = skeletonGraphic.Skeleton.FindBone(boneName);
            if (targetBone == null)
                Debug.LogError($"未找到名为 '{boneName}' 的骨骼！");
            else
            {
                Debug.Log($"找到名为 '{boneName}' 的骨骼！");
                // 记录骨骼初始局部位置作为圆心
                boneOrigin = new Vector2(targetBone.X, targetBone.Y);
                targetPosition = boneOrigin;
            }
        }
    }

    void Update()
    {
        if (targetBone == null) return;

        // 1. 获取 Canvas 的 RectTransform
        RectTransform canvasRect = skeletonGraphic.canvas.GetComponent<RectTransform>();
        if (canvasRect == null)
            return;

        //2. 根据 Canvas 渲染模式决定用于转换的摄像机
        Camera cam = null;
        if (skeletonGraphic.canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = skeletonGraphic.canvas.worldCamera;  // Screen Space - Camera 模式需要

        //3. 将鼠标屏幕坐标转换为 Canvas 局部坐标
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                Input.mousePosition,
                cam,
                out Vector2 localMousePos))
        {
            Debug.LogWarning("鼠标坐标转换失败");
            return;
        }

        //4. 将角色世界坐标转换为 Canvas 局部坐标
        Vector2 localCharPos = canvasRect.InverseTransformPoint(characterTransform.position);

        //5. 计算方向向量（从角色指向鼠标）
        Vector2 direction = localMousePos - localCharPos;
        if (direction.sqrMagnitude < 0.001f)  // 避免零向量
            return;

        Debug.Log(direction);
        //6. 计算反方向单位向量（骨骼移动方向：远离鼠标）
        Vector2 oppositeDir = -direction.normalized;

        //7. 计算目标位置：圆心 + 反方向 * 半径
        Vector3 desiredPos = localCharPos + oppositeDir * circleRadius;

        // 8. 应用位置
        targetBone.X = desiredPos.x;
        targetBone.Y = desiredPos.y;
        // Spine 会自动更新骨骼，无需手动调用
    }
}