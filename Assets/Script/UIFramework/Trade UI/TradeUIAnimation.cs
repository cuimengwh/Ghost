using DG.Tweening;
using Spine;
using Spine.Unity;
using UnityEngine;

public class TradeUIAnimation : MonoBehaviour
{
    [Header("组件获取")]
    public SkeletonGraphic skeletonGraphic;
    public RectTransform characterTransform;   // 角色 UI 的 RectTransform（用于计算鼠标方向）
    public GameObject tradePanel;              // 交易面板
    public CanvasGroup canvasGroup;              // 交易面板的 CanvasGroup（用于淡入淡出）

    [Header("骨骼设置")]
    public string boneName = "转向";

    [Header("圆圈移动范围")]
    public float circleRadius = 100f;           // 在骨骼父级局部空间中的半径

    [Header("平滑移动")]
    public bool smoothPosition = true;
    public float smoothSpeed = 10f;

    private Spine.Bone targetBone;
    private Vector2 boneOrigin;                  // 骨骼初始局部位置（圆心）
    private Vector2 currentTarget;                // 当前目标位置（平滑插值用）

    private Canvas canvas;
    private RectTransform canvasRect;
    private bool isAnimation = false;
    private TradeUI tradeUI => FindAnyObjectByType<TradeUI>();

    void Start()
    {
        if (skeletonGraphic == null || skeletonGraphic.Skeleton == null) return;
        targetBone = skeletonGraphic.Skeleton.FindBone(boneName);
        if (targetBone == null)
        {
            Debug.LogError($"未找到骨骼 '{boneName}'");
            return;
        }
        // 记录骨骼在其父骨骼局部空间中的初始位置作为圆心
        boneOrigin = new Vector2(targetBone.X, targetBone.Y);
        currentTarget = boneOrigin;

        canvas = skeletonGraphic.canvas;
        canvasRect = canvas.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (targetBone == null) return;

        //将屏幕坐标转换为 Canvas 局部坐标
        //注意：Overlay 模式下 camera 参数传 null
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                Input.mousePosition,
                null,                           // Overlay 模式不需要摄像机
                out Vector2 localMousePos))
        {
            Debug.LogWarning("无法将鼠标坐标转换到 Canvas 局部空间");
            return;
        }

        //将角色UI的世界坐标转换为Canvas局部坐标
        Vector2 localCharPos = canvasRect.InverseTransformPoint(characterTransform.position);

        //在Canvas局部空间中计算方向（从角色指向鼠标）
        Vector2 canvasDir = (localMousePos - localCharPos).normalized;
        if (canvasDir.sqrMagnitude < 0.001f) return;

        //骨骼应远离鼠标，因此取反方向
        Vector2 oppositeCanvasDir = -canvasDir;

        //将Canvas局部方向转换为世界方向
        // Canvas 的变换包含了旋转/缩放，TransformDirection 将局部方向转换为世界方向
        Vector3 worldDir = canvas.transform.TransformDirection(new Vector3(oppositeCanvasDir.x, oppositeCanvasDir.y, 0));

        //将世界方向转换到目标骨骼的父骨骼局部空间
        Spine.Bone parentBone = targetBone.Parent;
        Vector2 localDir;

        if (parentBone != null)
        {
            //通过父骨骼的 WorldToLocal 将世界方向转换为父骨骼局部方向
            //技巧：计算两个点的差值来得到方向
            Vector2 worldOrigin = Vector2.zero;
            Vector2 worldDirPoint = new Vector2(worldDir.x, worldDir.y);
            Vector2 localOrigin = parentBone.WorldToLocal(worldOrigin);
            Vector2 localDirPoint = parentBone.WorldToLocal(worldDirPoint);
            localDir = (localDirPoint - localOrigin).normalized;
        }
        else
        {
            //直接将世界方向转换到 skeletonGraphic 的局部空间
            Vector3 localDir3 = skeletonGraphic.transform.InverseTransformDirection(worldDir);
            localDir = new Vector2(localDir3.x, localDir3.y).normalized;
        }

        //计算目标位置（父骨骼局部空间）
        Vector2 targetPos = boneOrigin + localDir * circleRadius;

        // 可选：观察 localDir 是否随鼠标变化（调试用）
        // Debug.Log(localDir);

        //平滑处理
        if (smoothPosition)
        {
            currentTarget = Vector2.Lerp(currentTarget, targetPos, smoothSpeed * Time.deltaTime);
        }
        else
        {
            currentTarget = targetPos;
        }

        //应用位置
        targetBone.X = currentTarget.x;
        targetBone.Y = currentTarget.y;

        //强制Spine立即更新骨骼（通常不需要每帧调用，但如果你发现骨骼显示延迟可取消注释）
        //skeletonGraphic.Skeleton.UpdateWorldTransform();
    }

    public void OpenAndCloseTradePanel()
    {
        if (tradePanel != null)
        {
            if (!tradePanel.activeSelf)
            {
                tradePanel.SetActive(true);
                isAnimation = true;
                skeletonGraphic.DOFade(1f, 0.5f);
                canvasGroup.DOFade(1f, 0.5f).OnComplete(() =>
                {
                    canvasGroup.blocksRaycasts = true; // 确保面板可交互
                    isAnimation = false;

                    tradeUI.InitItemData();
                });
            }
            else
            {
                isAnimation = true;
                canvasGroup.blocksRaycasts = false;

                skeletonGraphic.DOFade(0f, 0.5f);
                canvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
                {
                    tradePanel.SetActive(false);
                    isAnimation = false;
                });
            }
        }
    }

    public void PlayTradeAnimation()
    {
        // 获取当前动画状态
        var state = skeletonGraphic.AnimationState;

        // 设置“zhayan”动画（假设它是不循环的，播放一次）
        // 注意：第三个参数为 false 表示不循环
        TrackEntry track = state.SetAnimation(0, "zhayan", false);

        // 监听动画完成事件
        track.Complete += (entry) =>
        {
            // 在动画完成时，播放 Idle 动画（可以循环或单次，根据需求）
            // 此处假设 Idle 是循环动画
            state.SetAnimation(0, "idle", true);
        };
    }
}