using System.Collections.Generic;
using UnityEngine;

namespace HexaBlast
{
   class FigureChecker : UnityCommon.MonoBehaviourSingleton<FigureChecker>
   {
      [SerializeField] FigureCheckBehaviour[] m_popCheckers;

      public bool CanPopSwapped(Block b0, Block b1)
      {
         if (AnyFigureCheck(b0))
         {
            return true;
         }
         else if (AnyFigureCheck(b1))
         {
            return true;
         }

         return false;
      }

      bool AnyFigureCheck(Block src)
      {
         foreach (var checker in m_popCheckers)
         {
            if (checker.Check(src))
            {
               return true;
            }
         }
         return false;
      }

      public bool FigureCheck(Block src, HashSet<Block> affecteds = null, List<ItemCreateInfo> items = null)
      {
         bool isFigured = false;
         foreach (var checker in m_popCheckers)
         {
            bool ok = checker.Check(src, affecteds, items);
            isFigured |= ok;
         }

         return isFigured;
      }
   }
}