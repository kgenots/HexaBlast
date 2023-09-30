using System.Collections.Generic;
using UnityEngine;

namespace HexaBlast
{
   /// <summary>
   /// 대각선 체크
   /// </summary>
   class SlashFigureCheck : FigureCheckBehaviour
   {
      [SerializeField] int m_minCondition = 3;
      [SerializeField] int m_itemCondition = 4;
      [SerializeField] BlockEntity m_leftUpItem = null;
      [SerializeField] BlockEntity m_rightUpItem = null;
      [SerializeField] BlockEntity m_verticalItem = null;

      private void OnValidate()
      {
         if (m_itemCondition < m_minCondition)
            m_itemCondition = m_minCondition;
      }

      // src 좌상, 좌하 끝으로 이동하여 개수 세기
      public override bool Check(Block src, HashSet<Block> figures = null, List<ItemCreateInfo> items = null)
      {
         // todo : use list pool
         List<Block> collect0 = new List<Block>();
         List<Block> collect1 = new List<Block>();
         List<Block> collect2 = new List<Block>();

         // src 좌상->우하
         FigureCheckTool.CollectFromTo(src, HexaDirection.LeftUp, HexaDirection.RightDown, collect0);

         // src 좌하->우상
         FigureCheckTool.CollectFromTo(src, HexaDirection.LeftDown, HexaDirection.RightUp, collect1);
         FigureCheckTool.CollectFromTo(src, HexaDirection.Up, HexaDirection.Down, collect2);

         // add to set
         if (figures != null)
         {
            if (collect0.Count >= m_minCondition)
            {
               foreach (Block b in collect0)
               {
                  figures.Add(b);
               }
            }

            if (collect1.Count >= m_minCondition)
            {
               foreach (Block b in collect1)
               {
                  figures.Add(b);
               }
            }

            if (collect2.Count >= m_minCondition)
            {
               foreach (Block b in collect2)
               {
                  figures.Add(b);
               }
            }
         }

         // add item
         if (items != null && collect0.Count >= m_itemCondition)
         {
            items.Add(new ItemCreateInfo()
            {
               Row = src.Row,
               Col = src.Col,
               ItemEntity = m_leftUpItem,
               ColorLayer = src.ColorLayer,
               Mergeds = collect0,
            });
         }

         if (items != null && collect1.Count >= m_itemCondition)
         {
            items.Add(new ItemCreateInfo()
            {
               Row = src.Row,
               Col = src.Col,
               ItemEntity = m_rightUpItem,
               ColorLayer = src.ColorLayer,
               Mergeds = collect1,
            });
         }

         if (items != null && collect2.Count >= m_itemCondition)
         {
            items.Add(new ItemCreateInfo()
            {
               Row = src.Row,
               Col = src.Col,
               ItemEntity = m_verticalItem,
               ColorLayer = src.ColorLayer,
               Mergeds = collect2,
            });
         }

         return collect0.Count >= m_minCondition ||
            collect1.Count >= m_minCondition;
      }
   }
}