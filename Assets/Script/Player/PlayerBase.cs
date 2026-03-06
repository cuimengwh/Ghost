using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerBase;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerBase : MonoBehaviour, IInteractable
{
    #region 枚举定义

    /// <summary>
    /// 角色物理状态枚举
    /// </summary>
    public enum CharacterPhysicalState
    {
        Idle,       // 待机状态
        Walking,    // 行走状态
        Running,    // 奔跑状态
        Jumping,    // 跳跃状态
        Sleeping,   // 睡眠状态
        fainting,   // 晕倒状态
        Falling,    // 下落状态
        Attacking,  // 攻击状态
        Planting,   // 种植状态
        Felling,    // 伐木状态
        Interacting,// 交互状态
        Dead        // 死亡状态
    }

    /// <summary>
    /// 移动方式枚举
    /// </summary>
    public enum MovementType
    {
        Walk,   // 行走
        Run,    // 奔跑
        squat,  // 下蹲
        Swim,   // 游泳
        Fly     // 飞行
    }

    /// <summary>
    /// 存在状态枚举
    /// </summary>
    public enum ExistenceState
    {
        Physical,   // 物理存在状态（实体状态）
        Psychical   // 精神存在状态（灵魂状态）
    }

    /// <summary>
    /// 角色可执行动作枚举
    /// </summary>
    public enum CharacterExecutableActions
    {
        nothing // 无动作
    }

    #endregion

    #region 公开属性：角色属性

    [Header("基础属性")]
    [SerializeField] protected float maxHealth = 100f;                // 最大生命值
    [SerializeField] protected float health = 100f;                   // 当前生命值
    [SerializeField] protected float maxEnergy = 100f;                // 最大精力值
    [SerializeField] protected float energy = 100f;                   // 当前精力值
    [SerializeField] protected float maxSpiritualNourishment = 100f;  // 最大灵魂滋养值
    [SerializeField] protected float spiritualNourishment = 100f;     // 当前灵魂滋养值

    [Header("移动设置")]
    [SerializeField] protected float walkSpeed = 5f;      // 行走速度
    [SerializeField] protected float runSpeed = 10f;      // 奔跑速度
    [SerializeField] protected float squatSpeed = 2.5f;   // 下蹲移动速度
    [SerializeField] protected float jumpHeight = 2f;     // 跳跃高度
    [SerializeField] protected float gravity = -9.81f;    // 重力系数
    [SerializeField] protected float rotationSpeed = 10f; // 旋转速度

    [Header("状态设置")]
    [SerializeField] private float interactingEnergyDrain = 1f; // 交互时精力消耗速率

    // 属性公开器
    public CharacterExecutableActions CurrentCharacterExecutableActions; // 当前可执行动作状态

    #endregion

    #region 私有字段

    private ExistenceState currentExistenceState = ExistenceState.Physical; // 当前存在状态

    // 输入缓存字段
    protected float horizontalInput;    // 水平输入轴值
    protected float verticalInput;      // 垂直输入轴值
    protected bool jumpInput;           // 跳跃输入标志
    protected bool runInput;            // 奔跑输入标志
    protected bool squatInput;          // 下蹲输入标志
    //private bool attackInput;       // 攻击输入标志（已注释）
    protected bool interactInput;       // 交互输入标志

    #endregion

    #region 事件

    /// <summary>
    /// 存在状态改变时触发的事件
    /// </summary>
    public event Action<ExistenceState> OnExistenceStateChanged;

    #endregion

    #region Unity生命周期方法

    // 注：此处预留Unity标准方法位置（如Update、Start等）
    // 实际使用时需根据游戏逻辑添加相应方法实现

    #endregion

    #region 状态切换方法

    /// <summary>
    /// 切换存在状态（物理/精神）
    /// </summary>
    protected virtual void ToggleExistenceState()
    {
        // 确定新状态：当前为物理状态则切换到精神状态，反之亦然
        ExistenceState newState = currentExistenceState == ExistenceState.Physical ?
            ExistenceState.Psychical : ExistenceState.Physical;

        // 执行状态切换
        ChangeExistenceState(newState);
    }

    /// <summary>
    /// 改变存在状态
    /// </summary>
    /// <param name="newState">目标状态</param>
    protected virtual void ChangeExistenceState(ExistenceState newState)
    {
        // 如果状态未改变则直接返回
        if (currentExistenceState == newState) return;

        // 执行旧状态退出逻辑
        ExitExistenceState(currentExistenceState);

        // 执行新状态进入逻辑
        EnterExistenceState(newState);

        // 更新当前状态并触发事件
        currentExistenceState = newState;
        OnExistenceStateChanged?.Invoke(newState);
    }

    /// <summary>
    /// 进入特定存在状态时的处理
    /// </summary>
    /// <param name="state">要进入的状态</param>
    protected virtual void EnterExistenceState(ExistenceState state)
    {
        // 根据具体状态执行相应的进入逻辑
        switch (state)
        {
            case ExistenceState.Physical:
                // 物理状态进入逻辑
                break;

            case ExistenceState.Psychical:
                // 精神状态进入逻辑
                break;
        }
    }

    /// <summary>
    /// 退出特定存在状态时的处理
    /// </summary>
    /// <param name="state">要退出的状态</param>
    protected virtual void ExitExistenceState(ExistenceState state)
    {
        // 根据具体状态执行相应的退出逻辑
        // 注：此处可添加状态切换时的清理工作
    }

    #endregion

    #region 输入处理

    /// <summary>
    /// 获取玩家输入
    /// </summary>
    protected virtual void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");           // 获取水平输入
        verticalInput = Input.GetAxis("Vertical");               // 获取垂直输入
        jumpInput = Input.GetButtonDown("Jump");                 // 检测跳跃按键
        runInput = Input.GetKey(KeyCode.LeftShift);              // 检测左Shift键（奔跑）
        squatInput = Input.GetKey(KeyCode.LeftControl);          // 检测左Ctrl键（下蹲）
        //attackInput = Input.GetMouseButtonDown(0);             // 检测鼠标左键（攻击）
        interactInput = Input.GetKeyDown(KeyCode.F);             // 检测F键（交互）
    }

    /// <summary>
    /// 实现IInteractable接口的交互方法
    /// </summary>
    /// <param name="interactor">交互发起者</param>
    void IInteractable.Interact(object interactor)
    {
        throw new NotImplementedException(); // 待具体实现
    }

    #endregion

    #region 接口声明

    /// <summary>
    /// 可交互接口定义
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// 交互方法
        /// </summary>
        /// <param name="interactor">交互发起者对象</param>
        void Interact(object interactor);
    }

    #endregion
}