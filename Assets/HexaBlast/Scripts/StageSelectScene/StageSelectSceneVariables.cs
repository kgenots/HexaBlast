using UnityEngine;

namespace HexaBlast.StageSelectScene
{
   class StageSelectSceneVariables : UnityCommon.MonoBehaviourSingleton<StageSelectSceneVariables>
   {
      public bool IsDragging;
      public float PopupAnimationSec = 0.3f;
   }
}