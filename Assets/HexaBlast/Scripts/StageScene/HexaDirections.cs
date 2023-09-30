using System.Collections.Generic;
using UnityEngine;

namespace HexaBlast
{
   static class HexaDirections
   {
      public static readonly IReadOnlyList<HexaDirection> DownDirection = new HexaDirection[]
      {
         HexaDirection.Down,
         HexaDirection.LeftDown,
         HexaDirection.RightDown,
      };

      public static readonly IReadOnlyList<(int row, int col)> EvenDelta = new (int, int)[6]
      {
         (1, 0), (0, -1), (0, +1), (-1, -1), (-1, +1), (-1, 0),
      };

      public static readonly IReadOnlyList<(int row, int col)> OddDelta = new (int, int)[6]
      {
         (1, 0), (+1, -1), (+1, +1), (0, -1), (0, +1), (-1, 0),
      };

      public static IReadOnlyList<(int row, int col)> GetDelta(int col)
      {
         return col % 2 == 0 ? EvenDelta : OddDelta;
      }
   }
}