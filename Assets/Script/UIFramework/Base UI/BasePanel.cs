using UnityEngine;

/// <summary>
/// 所有UI面板的父类，包含UI面板的状态信息
/// </summary>
public class BasePanel
{
    /// <summary>
    /// UI信息
    /// </summary>
    public UIType UIType { get; private set; }

    /// <summary>
    /// UI管理工具
    /// </summary>
    public UITool UITool { get; set; }

    public PanelManager PanelManager { get; set; }

    public UIManager UIManager { get; set; }

    /// <summary>
    /// 初始化UITool
    /// </summary>
    /// <param name="tool"></param>
    public void Initialize(UITool tool)
    {
        UITool = tool;
    }

    public void Initialize(PanelManager panelManager, UIManager uiManager)
    {
        PanelManager = panelManager;
        UIManager = uiManager;
    }

    public BasePanel(UIType uiType)
    {
        UIType = uiType;
    }

    /// <summary>
    /// UI进入时执行的操作，只会执行一次
    /// </summary>
    public virtual void OnEnter()
    {

    }

    /// <summary>
    /// UI暂停时执行的操作
    /// </summary>
    public virtual void OnPause() 
    {
        UITool.GetOrAddComponent<CanvasGroup>().blocksRaycasts = false; //暂停时不接受点击事件
    }

    /// <summary>
    /// UI继续时执行的操作
    /// </summary>
    public virtual void OnResume()
    {
        UITool.GetOrAddComponent<CanvasGroup>().blocksRaycasts = true; //恢复点击
    }

    /// <summary>
    /// UI退出时执行的操作
    /// </summary>
    public virtual void OnExit()
    {
        UIManager.DestroyUI(UIType);
    }

    /// <summary>
    /// 显示一个面板
    /// </summary>
    /// <param name="panel"></param>
    public void Push(BasePanel panel) => PanelManager?.Push(panel);

    /// <summary>
    /// 关闭一个面板
    /// </summary>
    public void Pop() => PanelManager?.Pop();
}
