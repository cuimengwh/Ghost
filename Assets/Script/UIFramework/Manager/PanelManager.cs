using System.Collections.Generic;
using UnityEngine;

public class PanelManager
{
    private Stack<BasePanel> stackPanel; //存储UI面板的栈
    private BasePanel panel;
    private UIManager uiManager;

    public PanelManager()
    {
        stackPanel = new Stack<BasePanel>();
        uiManager = new UIManager();
    }

    /// <summary>
    /// UI的入栈操作，此操作会显示一个面板，暂停当前面板
    /// </summary>
    /// <param name="nextPanel">要显示的面板</param>
    public void Push(BasePanel nextPanel)
    {
        if(stackPanel.Count > 0)
        {
            panel = stackPanel.Peek();
            panel.OnPause();
        }

        stackPanel.Push(nextPanel);
        GameObject panelGo = uiManager.GetSingleUI(nextPanel.UIType);

        nextPanel.Initialize(new UITool(panelGo));
        nextPanel.Initialize(this, uiManager);
        nextPanel.OnEnter();
    }

    /// <summary>
    /// 执行面板的出栈操作，此操作会关闭当前面板，恢复上一个面板
    /// </summary>
    public void Pop()
    {
        if(stackPanel.Count == 0)
        {
            Debug.LogError("栈中没有UI面板了");
            return;
        }
        if(stackPanel.Count > 0)
            stackPanel.Pop().OnExit();
        if(stackPanel.Count > 0)
            stackPanel.Peek().OnResume();
    }

    public void PopAll()
    {
        while(stackPanel.Count > 0)
        {
            stackPanel.Pop().OnExit();
        }
    }
}