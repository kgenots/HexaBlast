using System.Threading.Tasks;
using UnityEngine;

namespace HexaBlast
{
   static class InputTool
   {
      public static async Task WaitInputToClick(float waitSec)
      {
         float t = 0;
         while (t < waitSec)
         {
            if (Input.GetMouseButton(0))
            {
               break;
            }

            await Task.Yield();
            t += Time.deltaTime;
         }
      }
   }
}