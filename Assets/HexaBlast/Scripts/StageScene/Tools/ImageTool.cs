using UnityEngine;
using UnityEngine.UI;

namespace HexaBlast
{
   static class ImageTool
   {
      public static Vector2 GetTruePixelSize(Graphic image)
      {
         float scaleFactor = 1f;

         foreach (var canvas in image.GetComponentsInParent<Canvas>())
         {
            scaleFactor *= canvas.scaleFactor;
         }

         return image.rectTransform.rect.size * scaleFactor;
      }
   }
}