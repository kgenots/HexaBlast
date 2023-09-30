﻿using System.Collections.Generic;
using UnityEngine;

namespace HexaBlast
{
   abstract class FigureCheckBehaviour : MonoBehaviour
   {
      // todo : disable another popcheck when this fitten (this frame)
      // with order

      /// <summary>
      /// Src 기준 팝 모양 검사, 아이템 생성도 관리.
      /// figures, items both can be null
      /// </summary>
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