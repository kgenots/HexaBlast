using System.Collections.Generic;
using UnityEngine;

namespace HexaBlast
{
   abstract class FigureCheckBehaviour : MonoBehaviour
   {
      public abstract bool Check(Block src, HashSet<Block> figures = null, List<ItemCreateInfo> items = null);
   }

   struct ItemCreateInfo
   {
      public int Row;
      public int Col;
      public BlockEntity ItemEntity;
      public ColorLayer ColorLayer;
      public List<Block> Mergeds;
   }
}