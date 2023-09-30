﻿using UnityEngine;

namespace HexaBlast.StageSelectScene
{
   class StageSelectSceneCamFocus : MonoBehaviour
   {
      public float z = -10;

      private void OnEnable()
      {
         var nextStage = GlobalVariables.Instance.CurrentStage?.NextStage;

         if (nextStage)
         {
            var trophy = TrophyObjects.Instance.GetTrophyObject(nextStage);
            if (trophy)
            {
               var dst = trophy.transform.position;
               dst.x = 0;
               dst.z = z;
               transform.position = dst;
            }

            trophy?.OnMouseUp();
         }
         else
         {
            var trophy = TrophyObjects.Instance.GetTrophyObject(nextStage);
            if (trophy)
            {
               var dst = trophy.transform.position;
               dst.x = 0;
               dst.z = z;
               transform.position = dst;
            }
         }
      }
   }
}