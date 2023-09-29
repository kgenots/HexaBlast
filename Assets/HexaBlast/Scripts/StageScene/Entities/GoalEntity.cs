using UnityEngine;

namespace HexaBlast
{
   [CreateAssetMenu(fileName = "New Goal", menuName = "Entity/Goal")]
   class GoalEntity : ScriptableObject
   {
      public Texture2D Image;
      public string ColorName;
   }
}