using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace HexaBlast.Scenario.StageScene
{
   class MatchState : State
   {
      [SerializeField] Camera m_cam = null;
      [SerializeField] float m_dragStartDst = 0.5f;
      [SerializeField] float m_swapAnimationSec = 0.4f;
      [SerializeField] bool m_canMoveWhenCountZero;

      public override void Initialize()
      {

      }

      public override void BeginState()
      {
         if (m_cam == null)
            m_cam = Camera.main;

         GetInputAsync().Watch();
      }

      void OnDrawZizmos()
      {
         Gizmos.DrawLine(beg, end - beg);
      }

      Vector2 beg = default;
      Vector2 end = default;

      async Task GetInputAsync()
      {
         bool isTouchPressed = false;

         while (enabled)
         {
            if (Input.GetMouseButton(0))
            {
               Vector2 spos = (Input.touchCount == 0) ? (Vector2)Input.mousePosition : Input.GetTouch(0).position;
               Vector2 wpos = m_cam.ScreenToWorldPoint(spos);

               if (Input.GetMouseButtonDown(0))
               {
                  beg = wpos;
                  isTouchPressed = true;
               }

               end = wpos;
            }

            if (Input.GetMouseButtonUp(0) && isTouchPressed)
            {
               Vector2 posDelta = end - beg;
               if (posDelta.sqrMagnitude > m_dragStartDst)
               {
                  Block a = null;
                  Block b = null;
                  bool ok = TryGetBlocksFromDrag(beg, end, ref a, ref b);
                  if (ok && a.Entity.CanSwap && b.Entity.CanSwap)
                  {
                     if (m_canMoveWhenCountZero)
                     {
                        SwapAsync(a, b).Watch();
                        return;
                     }
                     else if (Stage.Instance.RemainMove > 0)
                     {
                        SwapAsync(a, b).Watch();
                        return;
                     }
                  }
                  else
                  {
                     beg = end = default;
                  }
               }
               else
               {
               }
            }

            await Task.Yield();
         }
      }

      bool TryMerge(Block src, Block dst)
      {
         if (src.Entity.CanMerge && dst.Entity.CanMerge)
         {

            StateDatas.Instance.AbsolutePopBlocks.Add(src);
            StateDatas.Instance.AbsolutePopBlocks.Add(dst);

            return true;
         }

         return false;
      }

      async Task SwapAsync(Block src, Block dst)
      {
         var srcRow = src.Row;
         var srcCol = src.Col;
         var dstRow = dst.Row;
         var dstCol = dst.Col;
         var srcPos = src.transform.position;
         var dstPos = dst.transform.position;

         var stdAni = new Simation(srcPos, dstPos, src.transform, Simation.EaseInOutCurve, m_swapAnimationSec);
         var dtsAni = new Simation(dstPos, srcPos, dst.transform, Simation.EaseInOutCurve, m_swapAnimationSec);
         stdAni.Start();
         dtsAni.Start();
         await stdAni.AsyncTask;
         await dtsAni.AsyncTask;
         BlockMap.Instance.SetBlock(srcRow, srcCol, dst);
         BlockMap.Instance.SetBlock(dstRow, dstCol, src);

         if (TryMerge(src, dst))
         {
            Stage.Instance.AddRemainMoveDelta(-1);

            StateManager.Instance.ChangeState(StateType.Pop);
            return;
         }

         bool canPop = FigureChecker.Instance.CanPopSwapped(src, dst);
         if (canPop)
         {
            var datas = StateDatas.Instance;
            datas.SwapPopChecks.Add(src);
            datas.SwapPopChecks.Add(dst);

            Stage.Instance.AddRemainMoveDelta(-1);

            StateManager.Instance.ChangeState(StateType.Pop);
         }
         else
         {
            stdAni = new Simation(srcPos, dstPos, dst.transform, Simation.EaseInOutCurve, m_swapAnimationSec);
            dtsAni = new Simation(dstPos, srcPos, src.transform, Simation.EaseInOutCurve, m_swapAnimationSec);
            stdAni.Start();
            dtsAni.Start();
            await stdAni.AsyncTask;
            await dtsAni.AsyncTask;
            BlockMap.Instance.SetBlock(srcRow, srcCol, src);
            BlockMap.Instance.SetBlock(dstRow, dstCol, dst);

            GetInputAsync().Watch();
         }
      }

      bool TryGetBlocksFromDrag(Vector2 beg, Vector2 end, ref Block src, ref Block dst)
      {
         var map = BlockMap.Instance;
         dst = null;

         var (row, col) = map.WorldPosToIndex(beg.x, beg.y);
         if (map.IsIndexOutOfRange(row, col)) return false;
         if (!map.IsIndexEnabled(row, col)) return false;
         src = map.GetBlock(row, col);
         if (src == null) return false;

         var minDist = 1e9f;
         var delta = HexaDirections.GetDelta(col);
         for (int k = 0; k < delta.Count; ++k)
         {
            int nrow = row + delta[k].row;
            int ncol = col + delta[k].col;
            if (map.IsIndexOutOfRange(nrow, ncol)) continue;
            if (!map.IsIndexEnabled(nrow, ncol)) continue;
            var block = map.GetBlock(nrow, ncol);
            if (block == null) continue;

            var wpos = (Vector2)map.IndexToWorldPos(nrow, ncol);
            float dist = (end - wpos).sqrMagnitude;

            if (dist < minDist)
            {
               minDist = dist;
               dst = block;
            }
         }

         return dst != null;
      }
   }
}