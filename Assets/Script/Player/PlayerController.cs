using UnityEngine;

/// <summary>
/// 玩家控制器类，继承自PlayerBase，负责处理玩家移动、旋转、动画等行为
/// </summary>
[RequireComponent(typeof(CharacterController))] // 确保游戏对象上有CharacterController组件
public class PlayerController : PlayerBase
{
    private CharacterController characterController; // 角色控制器组件引用
    private Animator animator;                       // 动画控制器组件引用
    [Header("UI对象")]
    public GameObject myBag;                     // 玩家背包对象引用
    public bool bagOpen = false;                   // 背包打开状态标志
    private Vector3 velocity;                        // 当前速度向量（包含水平和垂直方向）
    private Vector3 moveDir;                         // 移动方向向量（注意：此变量在代码中未被正确赋值）
    private float MoveSpeed;                         // 移动速度变量（注意：此变量在代码中未被使用）

    // 初始化方法，在对象创建时调用
    protected void Awake()
    {
        // 获取必要的组件引用
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>(); // 在子对象中查找Animator组件
        
    }

    // 每帧更新方法
    void Update()
    {
        GetInput();           // 获取玩家输入
        HandleMovement();     // 处理移动逻辑
        HandleGravity();      // 处理重力与跳跃
        HandleRotation();     // 处理角色旋转
        //HandleAnimation();    // 处理动画状态
    }

    // 处理角色移动的方法
    private void HandleMovement()
    {
        // 获取水平与垂直输入轴值
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(horizontal, 0, vertical).normalized; // 归一化输入向量

        // 基于摄像机方向计算移动方向
        Transform cam = Camera.main.transform;
        Vector3 forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;  // 相机前向投影到水平面
        Vector3 right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;  // 相机右向投影到水平面
        Vector3 direction = forward * input.z + right * input.x;  // 计算基于相机方向的最终移动方向

        // 根据按键状态确定移动速度
        float speed;
        if (Input.GetKey(KeyCode.LeftShift)) // 按下左Shift键时奔跑
        {
            speed = runSpeed;        // 使用奔跑速度
        }
        else if (Input.GetKey(KeyCode.LeftControl)) // 按下左Ctrl键时下蹲
        {
            speed = squatSpeed;      // 使用下蹲速度
        }
        else // 默认状态
        {
            speed = walkSpeed;       // 使用行走速度
        }
        velocity = direction * speed;  // 计算速度向量

        // 应用移动
        characterController.Move(velocity * Time.deltaTime);

        // 当有输入时立即转向移动方向（注意：这会覆盖HandleRotation方法的平滑旋转效果）
        if (direction.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    // 处理重力与跳跃的方法
    private void HandleGravity()
    {
        gravity = 0; // 重力值设为0（测试用，实际游戏中应使用物理重力值）

        // 接地检测：如果角色在地面上且垂直速度向下，则重置垂直速度
        if (characterController.isGrounded && velocity.y < 0)
            velocity.y = -2f;  // 轻微向下力确保玩家保持在地面

        // 处理跳跃逻辑
        if (jumpInput && characterController.isGrounded) // 如果按下跳跃键且角色在地面上
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);  // 计算跳跃初速度
        else if (jumpInput) // 如果按下跳跃键但不在地面上
            Debug.Log("无法跳跃，当前不在地面上");  // 输出跳跃失败提示

        // 应用重力加速度
        velocity.y += gravity * Time.deltaTime;

        // 应用垂直方向移动（注意：这里重复调用了Move方法，与HandleMovement中的调用可能冲突）
        characterController.Move(velocity * Time.deltaTime);
    }

    // 处理角色旋转的方法
    private void HandleRotation()
    {
        // 平滑旋转朝向移动方向（注意：moveDir变量未被HandleMovement更新，此方法可能无法正常工作）
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(moveDir); // 创建目标旋转
            // 使用球面插值平滑旋转到目标方向
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSpeed * Time.deltaTime);
        }
    }

    // 处理动画状态的方法
    //public virtual void HandleAnimation()
    //{
    //    if (animator == null) return; // 如果动画器不存在则直接返回

    //    // 设置奔跑状态动画参数
    //    if (Input.GetKey(KeyCode.LeftShift) && velocity.magnitude > 0.1f)
    //    {
    //        animator.SetBool("isRunning", true); // 设置为奔跑状态
    //    }
    //    else
    //    {
    //        animator.SetBool("isRunning", false); // 取消奔跑状态
    //    }

    //    // 设置速度参数控制混合树
    //    animator.SetFloat("Speed", velocity.magnitude);
    //}

    private void CursorLock(bool visible)
    {
        if (visible)
        {
            Cursor.lockState = CursorLockMode.None; // 释放光标
            Cursor.visible = true;                   // 显示光标
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // 锁定光标
            Cursor.visible = false;                    // 隐藏光标
        }
    }
}