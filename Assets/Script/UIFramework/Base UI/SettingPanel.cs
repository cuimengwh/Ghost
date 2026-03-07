using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    static readonly string path = "Prefabs/UI/Setting Panel";

    public SettingPanel() : base(new UIType(path)) { }

    public override void OnEnter()
    {
        base.OnEnter();

        UITool.GetOrAddComponentInChildren<Button>("Back Button").onClick.AddListener(() =>
        {
            //点击事件可以加在这里
            Debug.Log("返回开始界面");
            Pop();
        });
    }
}
