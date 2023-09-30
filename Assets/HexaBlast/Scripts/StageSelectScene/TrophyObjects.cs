using UnityEngine;

namespace HexaBlast.StageSelectScene
{
   class TrophyObjects : UnityCommon.MonoBehaviourSingleton<TrophyObjects>
   {
      public TrophyObject GetTrophyObject(StageEntity stage)
      {
         foreach (var child in GetComponentsInChildren<TrophyObject>())
         {
            if (child.StageEntity == stage)
            {
               return child;
            }
         }

         return null;
      }
   }
}