using System.Threading.Tasks;

namespace Utopia.Core
{
    /// <summary>
    /// 数据持久化接口
    /// 定义了需要保存和加载数据的对象的通用行为。
    /// 使用了泛型来支持不同类型的数据结构。
    /// </summary>
    /// <typeparam name="T">用于存储游戏状态的数据类型</typeparam>
    public interface IDataPersistence<T> where T : class
    {
        /// <summary>
        /// 当数据发生更改且尚未保存时，应返回 true。
        /// </summary>
        bool HasUnsavedChanges { get; }

        /// <summary>
        /// 从数据载体中加载状态。
        /// </summary>
        /// <param name="data">包含游戏状态的数据对象</param>
        Task LoadData(T data);

        /// <summary>
        /// 将当前状态保存到数据载体中。
        /// </summary>
        /// <param name="data">用于存储游戏状态的数据对象</param>
        Task SaveData(T data);
    }
}