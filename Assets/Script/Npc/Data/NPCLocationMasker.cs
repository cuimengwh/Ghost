using UnityEngine;

namespace Utopia.Npc
{

    public class NPCLocationMasker : MonoBehaviour
    {
        [Tooltip("对应日程表里的 locationID")]
        public string locationID;

        // 可以在 Scene 窗口画个球，方便看位置
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, 0.3f);
        }
    }
}
