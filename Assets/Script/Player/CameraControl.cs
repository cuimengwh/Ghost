using UnityEngine;

/// <summary>
/// 相机跟随脚本，挂在主相机上，实现第三人称视角的跟随效果。
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;        // 角色
    public float eyeHeight = 1.6f;  // 角色眼睛高度

    [Header("相机参数")]
    public float distance = 4f;         // 默认距离
    public float minDistance = 2f;      // 最近距离
    public float maxDistance = 6f;      // 最远距离
    public float zoomSpeed = 2f;        // 滚轮缩放速度
    public float followSpeed = 10f;     // 跟随平滑度
    public float rotationSpeed = 8f;    // 旋转平滑度

    [Header("鼠标控制")]
    public float mouseSensitivity = 2f;
    public float minPitch = -30f;  // 俯视下限（可看到脚下）
    public float maxPitch = 70f;   // 仰视上限（可看到天空）

    // 存储当前的欧拉角：yaw为水平旋转角度（绕Y轴），pitch为垂直旋转角度（绕X轴）
    private float yaw = 0f;
    private float pitch = 20f; // 默认轻微俯视

    void Start()
    {
        // 锁定鼠标光标至窗口中心并隐藏，提供无缝的鼠标控制体验
        Cursor.lockState = CursorLockMode.Locked;

        // 初始化相机角度，从当前的旋转欧拉角获取初始值
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;      // 绕Y轴的角度（左右旋转）
        pitch = angles.x;    // 绕X轴的角度（上下旋转）
    }

    // LateUpdate 在所有 Update 执行完后调用，确保相机跟随在角色移动之后，避免抖动
    void LateUpdate()
    {
        if (target == null) return; // 未设置目标时跳过

        // 1. 鼠标控制：根据鼠标移动增量更新旋转角度
        // Mouse X/Y 轴对应鼠标横向和纵向移动，乘以灵敏度调整速度
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity; // 减号使鼠标向上推时相机仰视（符合直觉）
        // 限制 pitch 角度范围，避免相机转到角色下方或过头顶翻转
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // 2. 滚轮缩放：根据滚轮输入动态改变相机与目标的距离
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            distance -= scroll * zoomSpeed;      // 向上滚动减少距离（拉近），向下滚动增加距离（拉远）
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // 3. 计算相机期望位置（基于球面坐标系）
        // 先确定观察中心点（角色的眼睛位置）
        Vector3 eyeCenter = target.position + Vector3.up * eyeHeight;
        // 根据当前的 yaw 和 pitch 构建旋转四元数
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        // 从观察中心沿相机背方向移动 distance 距离得到期望的相机位置
        // Vector3.forward 是 (0,0,1)，乘以旋转后得到朝向角色的反方向向量，再乘距离即为偏移量
        Vector3 desiredPos = eyeCenter - rotation * Vector3.forward * distance;

        // 4. 平滑移动与旋转：使用插值函数使相机运动更柔和，避免生硬的跳跃
        // 位置线性插值（Lerp），跟随速度 followSpeed 控制平滑程度
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
        // 旋转球形插值（Slerp），使得旋转变化均匀且自然
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }
}