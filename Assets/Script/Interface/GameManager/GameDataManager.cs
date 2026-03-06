using System.Threading.Tasks;

namespace Utopia.Core
{
    /// <summary>
    /// 可存档系统接口
    /// 任何需要参与GameDataManager统一存档流程的管理器都必须实现此接口
    /// </summary>
    public interface ISaveableSystem
    {
        string SaveKey { get; }

        Task Save(string slotId);

        Task Load(string slotId);
    }
}