using UnityEngine;

namespace HexaBlast
{
   class CameraSetter : MonoBehaviour
   {
      public float rowPadding = 1f;
      public float colPadding = 1f;
      public bool m_updateCameraWithValidation;

      private void OnValidate()
      {
         if (m_updateCameraWithValidation)
         {
            FitCamera();
         }
      }

      [UnityCommon.InspectorButton("FitCamera")] public bool btn0;
      public void FitCamera()
      {
         var cam = Camera.main;
         if (!cam) return;

         var map = BlockMap.Instance;
         if (!map) return;

         var rowSize = 0f;
         var colSize = 0f;
         rowSize = map.RowSize * map.RowGap * (0.7f + rowPadding) * cam.aspect;
         colSize = map.ColSize * map.ColGap * (0.7f + colPadding);
         cam.orthographicSize = Mathf.Max(rowSize, colSize);
      }
   }
}