using UnityEditor;
using UnityEngine;

namespace HexaBlast
{
   [CreateAssetMenu(fileName = "New Socket", menuName = "Entity/Socket")]
   class SocketEntity : ScriptableObject
   {
      [SerializeField] BlockSocket m_prefab;
      [SerializeField] bool m_isSpawn;
      [SerializeField] bool m_canAffectedByAbove;
      [SerializeField] int m_initialHp = 0;
      [SerializeField] string m_colorName;
      [SerializeField] bool m_isGoal;

      public string ColorName => m_colorName;
      public BlockSocket Prefab => m_prefab;
      public bool IsSpawn => m_isSpawn;
      public int HP => m_initialHp;
      public bool IsGoal => m_isGoal;

      public bool CanAffectedByAbove => m_canAffectedByAbove;
   }
}