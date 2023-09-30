using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HexaBlast.Scenario.StageScene
{
   class DropState : State
   {
      [SerializeField] float m_blockDropSec;

      HashSet<Block> m_movedSet = new HashSet<Block>();

      public override void Initialize()
      {

      }

      public override void BeginState()
      {
         DropCycleAsync();
      }

      async void DropCycleAsync()
      {
         var map = BlockMap.Instance;
         var data = StateDatas.Instance;

         bool isDropped = true;
         while (isDropped && this && enabled)
         {
            DropBlocks(ref isDropped);

            CreateNewBlocks(ref isDropped);

            if (isDropped)
            {
               await Task.Delay((int)(m_blockDropSec * 1000));
            }
         }

         foreach (Block b in m_movedSet)
         {
            data.DropPopChecks.Add(b);

         }
         m_movedSet.Clear();

         await Task.Delay((int)(m_blockDropSec * 1000));
         if (!this || !enabled) return;

         StateManager.Instance.ChangeState(StateType.Pop);
      }

      void CreateNewBlocks(ref bool isDropped)
      {
         var map = BlockMap.Instance;
         var stage = Stage.Instance;

         foreach (var socket in map.SpawnSockets)
         {
            var row = socket.Row;
            var col = socket.Col;
            var nrow = row + 1;
            if (!map.IsIndexEnabled(row, col)) continue;
            if (map.IsIndexOutOfRange(nrow, col)) continue;
            if (!map.IsIndexEnabled(nrow, col)) continue;
            var belowBlock = map.GetBlock(nrow, col);
            if (belowBlock != null) continue;

            var entity = stage.GetNextDropBlock();
            var block = map.GetBlock(row, col);

            isDropped = TryDropSingle(block) | isDropped;
         }
      }

      void DropBlocks(ref bool isDropped)
      {
         var map = BlockMap.Instance;
         isDropped = false;

         for (int i = map.RowSize - 1; i >= 0; --i)
         {
            for (int j = 0; j < map.ColSize; ++j)
            {
               if (!map.IsIndexEnabled(i, j)) continue;
               var block = map.GetBlock(i, j);
               if (block == null) continue;

               isDropped = TryDropSingle(block) | isDropped;
            }
         }
      }

      bool TryDropSingle(Block block)
      {
         if (!block) return false;
         if (!block.Entity.CanDrop) return false;

         var map = BlockMap.Instance;
         int row = block.Row;
         int col = block.Col;
         var dirs = HexaDirections.DownDirection;
         var deltas = HexaDirections.GetDelta(col);

         foreach (var d in dirs)
         {
            int nr = row + deltas[(int)d].row;
            int nc = col + deltas[(int)d].col;
            if (map.IsIndexOutOfRange(nr, nc)) continue;
            if (!map.IsIndexEnabled(nr, nc)) continue;
            if (map.GetBlock(nr, nc) != null) continue;

            if (d == HexaDirection.LeftDown || d == HexaDirection.RightDown)
            {
               var canNotDrop = false;
               var deltas2 = HexaDirections.GetDelta(nc);
               int nnr = nr;
               while (true)
               {
                  nnr = nnr - 1;
                  if (map.IsIndexOutOfRange(nnr, nc)) break;
                  if (map.IsIndexNotEnabled(nnr, nc)) break;

                  if (map.GetSocket(nnr, nc).Entity.IsSpawn)
                  {
                     canNotDrop = true;
                     break;
                  }

                  var aboveBlock = map.GetBlock(nnr, nc);
                  if (aboveBlock != null)
                  {
                     if (aboveBlock.Entity.CanDrop)
                     {
                        canNotDrop = true;
                        break;
                     }
                     else
                     {
                        canNotDrop = false;
                        break;
                     }
                  }
               }

               if (canNotDrop)
               {
                  continue;
               }
            }

            Vector2 from = map.IndexToWorldPos(row, col);
            Vector2 to = map.IndexToWorldPos(nr, nc);
            var ani = new Simation(to, block.transform, m_blockDropSec);
            ani.Start();

            map.SetBlock(row, col, null);
            map.SetBlock(nr, nc, block);

            m_movedSet.Add(block);

            return true;
         }

         return false;
      }
   }
}