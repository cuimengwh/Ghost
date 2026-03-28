using System.Collections;
using System.Collections.Generic;
using DG.Tweening.Core.Easing;
using UnityEngine;
using Utopia.Npc;
using Utopia.Core.Event;
using Utopia.Core.Services;

/// <summary>
/// 情绪系统（Mood System）
/// 管理居民的情绪相关数值，包括恐慌（Panic）、孤独（Loneliness）等。
/// 对应设计文档中的“恐慌值”、“孤独”、“哀惧”机制。
/// 该类负责数值的增减、修正（根据性格标签）以及触发相应事件。
/// </summary>
public class MoodSystem
{
    private NPCRuntimeData _data;       // NPC运行时数据，包含恐慌值、孤独值等
    private NPCProfile _profile;         // NPC静态配置，包含性格标签

    private IEventManager _eventManager; // 事件管理器，用于发布情绪变化事件

    /// <summary>
    /// 增加恐慌值，并根据居民性格标签进行修正。
    /// 对应设计文档：
    /// - 悲观主义（Pessimist）：恐慌增长×1.5
    /// - 理性（Rational）：对话引起的恐慌增长×0.8
    /// 当恐慌值达到100且NPC尚未标记为死亡时，发布NPCPanicChangedEvent（可能导致灵魂消散）。
    /// </summary>
    /// <param name="baseAmount">基础增加量</param>
    /// <param name="isDialogue">是否由对话引起（用于理性特质修正）</param>
    public void AddPanic(float baseAmount, bool isDialogue = false)
    {
        float multiplier = 1f;
        // 悲观主义特质：恐慌值增加1.5倍
        if (_profile.optimismTrait == OptimismTrait.Pessimist) multiplier *= 1.5f;

        // 理性特质：对话引起的恐慌值增加减少20%
        if (isDialogue && _profile.rationalityTrait == RationalityTrait.Rational) multiplier *= 0.8f;

        // 更新恐慌值，限制在0~100之间
        _data.panicValue = Mathf.Clamp(_data.panicValue + baseAmount * multiplier, 0f, 100f);

        // 如果恐慌值达到100且当前NPC未被标记为死亡，发布事件
        // 设计文档：恐慌值达100时灵魂消散（彻底死亡）
        if (_data.panicValue >= 100f && !_data.isDead)
        {
            // 通过全局服务定位器获取事件管理器（服务定位器模式）
            _eventManager = ServiceLocatorProvider.Global.Locator.Get<IEventManager>();
            // 发布恐慌变化事件，其他系统（如状态机）可监听并处理死亡逻辑
            _eventManager.Publish(new NPCPanicMaxEvent(_data.npcID));
        }
    }

    /// <summary>
    /// 增加孤独值。
    /// 对应设计文档：
    /// - 复活后亲友未复活时，孤独值随时间增长。
    /// - 孤独值过高可能触发哀惧（群体孤独影响全农场产量）。
    /// </summary>
    /// <param name="value">增加的孤独值</param>
    public void AddLoneliness(float value)
    {
        // 更新孤独值，限制在0~100之间
        _data.lonelinessValue = Mathf.Clamp(_data.lonelinessValue + value, 0f, 100f);
        // TODO: 当孤独值达到阈值时，可触发哀惧状态或相关事件
    }

    /// <summary>
    /// 每帧更新情绪的自然变化（如恐慌自然消退、孤独随时间增长等）。
    /// 设计文档中提到恐慌值可随时间自然消退，孤独值随时间增长。
    /// 当前方法为空，可在此实现时间相关的数值衰减/增长。
    /// </summary>
    /// <param name="deltaTime">时间增量（秒）</param>
    public void UpDate(float time)
    {
        // 恐慌、孤独、能量的自然变化
        // 例如：恐慌值随时间缓慢减少（设计文档：恐慌值减少-随时间自然消退）
        // _data.panicValue = Mathf.Max(0, _data.panicValue - decayRate * time);
        // 孤独值随时间缓慢增加（如果亲友未复活）
        // _data.lonelinessValue = Mathf.Min(100, _data.lonelinessValue + increaseRate * time);
    }
}