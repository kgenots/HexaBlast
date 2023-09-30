﻿using UnityEngine;

namespace HexaBlast
{
   static class Vector3Extension
   {
      public static Vector3 Reciprocal(this Vector3 src)
      {
         src.x = 1 / src.x;
         src.y = 1 / src.y;
         src.z = 1 / src.z;
         return src;
      }
   }
}