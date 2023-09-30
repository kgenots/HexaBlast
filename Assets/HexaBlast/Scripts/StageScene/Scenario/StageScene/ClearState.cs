﻿using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HexaBlast.Scenario.StageScene
{
   class ClearState : State
   {
      enum ClearStep
      {
         ShowImage,
         ClearItems,
         ToyParty,
         clearMenu,
      }

      [SerializeField] Camera m_cam;
      [SerializeField] Image m_levelClearImage;
      [SerializeField] Image m_toyPartyImage;
      [SerializeField] ClearMenuPanel m_clearMenuPanel;

      [SerializeField] List<BlockEntity> m_toyPartyItems;
      [SerializeField] GameObject m_toyPartyCannonShot;
      [SerializeField] float m_toyPartyCannonShotSec;
      [SerializeField] float m_toyCannonOffset = 1.5f;
      [SerializeField] RectTransform m_toyCannonTrans;
      [SerializeField] RectTransform m_toyCannonDstTrans;
      [SerializeField] float m_screenImageShowDurSec = 0.4f;

      bool m_isNeverClearStage;
      ClearStep m_nextClearStep = ClearStep.ShowImage;

      public override void Initialize()
      {
         m_isNeverClearStage = false;
         m_nextClearStep = ClearStep.ShowImage;
         if (!m_cam) m_cam = Camera.main;
      }

      public override void BeginState()
      {
         if (Stage.Instance.GetRemainGoalCnt() <= 0 && !m_isNeverClearStage)
         {
            MoveNextClear();
         }
         else
         {
            StateManager.Instance.ChangeState(StateType.Match);
         }
      }

      async void MoveNextClear()
      {
         if (m_nextClearStep == ClearStep.ShowImage)
         {
            m_nextClearStep = ClearStep.ClearItems;

            await ShowTopBotImageAnimation(m_levelClearImage);
            MoveNextClear();
         }
         else if (m_nextClearStep == ClearStep.ClearItems)
         {
            ClearItems();
         }
         else if (m_nextClearStep == ClearStep.ToyParty)
         {
            m_nextClearStep = ClearStep.clearMenu;

            await StartToyParty();
         }
         else if (m_nextClearStep == ClearStep.clearMenu)
         {
            m_clearMenuPanel.Show();
         }
      }

      public void NeverClearStage()
      {
         m_isNeverClearStage = true;
      }

      [UnityCommon.InspectorButton("ShowClearImageAsync")] public bool btn;
      async Task ShowTopBotImageAnimation(Image src)
      {
         float approachSec = 0.5f;
         float fallbackSec = 0.3f;

         src.gameObject.SetActive(true);

         var screenToWorldFactor = CameraTool.ScreenToWorldFactor(m_cam);
         var imageSize = ImageTool.GetTruePixelSize(src);
         imageSize.Scale(screenToWorldFactor);

         var beg = CameraTool.WorldTopCenterPoint(m_cam) + Vector3.up * imageSize.y;
         var end = CameraTool.WorldCenterPoint(m_cam);

         var approachAni = new Simation(beg, end, src.transform, approachSec);
         approachAni.Start();

         await InputTool.WaitInputToClick(m_screenImageShowDurSec + approachSec);
         if (!approachAni.AsyncTask.IsCompleted)
            approachAni.Cancel();

         end = CameraTool.WorldBotCenterPoint(m_cam) - Vector3.up * imageSize.y * 1.5f;
         await new Simation(end, src.transform, fallbackSec).StartAsync();

         LazyDisableGameObject(src.gameObject, 2f);
      }

      async void LazyDisableGameObject(GameObject src, float sec)
      {
         await Task.Delay((int)(sec * 1000));

         if (src)
         {
            src.SetActive(false);
         }
      }

      void ClearItems()
      {
         var map = BlockMap.Instance;
         bool isItemExist = false;

         StateDatas.Instance.AsyncronouseItemAnimation = true;

         for (int i = 0; i < map.RowSize; ++i)
         {
            for (int j = 0; j < map.ColSize; ++j)
            {
               if (map.IsIndexNotEnabled(i, j)) continue;
               var b = map.GetBlock(i, j);
               if (b == null) continue;

               if (b.Entity.IsItem)
               {
                  StateDatas.Instance.AbsolutePopBlocks.Add(b);
                  isItemExist = true;
               }
            }
         }

         if (isItemExist)
         {
            StateManager.Instance.ChangeState(StateType.Pop);
         }
         else
         {
            m_nextClearStep = ClearStep.ToyParty;
            MoveNextClear();
         }
      }

      async Task StartToyParty()
      {
         var map = BlockMap.Instance;
         int remainMove = Stage.Instance.RemainMove;
         List<Block> blocks = new List<Block>();
         for (int i = 0; i < map.RowSize; ++i)
         {
            for (int j = 0; j < map.ColSize; ++j)
            {
               if (map.IsIndexNotEnabled(i, j)) continue;
               var b = map.GetBlock(i, j);
               if (b == null) continue;
               blocks.Add(b);
            }
         }
         int changeCnt = Stage.Instance.RemainMove;
         changeCnt = Mathf.Min(blocks.Count, remainMove);

         if (changeCnt <= 0)
         {
            MoveNextClear();
            return;
         }

         await ShowTopBotImageAnimation(m_toyPartyImage);

         var beg = m_toyCannonTrans.position;
         var end = m_toyCannonDstTrans.position;
         await new Simation(beg, end, m_toyCannonTrans, 1.0f).StartAsync();

         await StartToyPartyItemChange(blocks, changeCnt);

         await new Simation(end, beg, m_toyCannonTrans, 0.5f).StartAsync();

         StateManager.Instance.ChangeState(StateType.Pop);
      }

      async Task StartToyPartyItemChange(List<Block> blocks, int changeCnt)
      {
         var map = BlockMap.Instance;

         float speedMulti = 1;
         while (changeCnt-- > 0)
         {
            var idx = Random.Range(0, blocks.Count);
            var block = blocks[idx];
            var row = block.Row;
            var col = block.Col;
            var color = block.ColorLayer;
            if (color == ColorLayer.None) color = ColorLayer.Purple;
            blocks.RemoveAt(idx);

            Stage.Instance.AddRemainMoveDelta(-1);
            await ToyPartyItemChangeAnimation(block, speedMulti);
            speedMulti *= 1.10f;

            map.RemoveBlock(block);
            BlockFactory.Instance.RemoveBlock(ref block);

            var item = BlockFactory.Instance.GetBlock(GetNextToyPartyItem());
            map.AddChildBlock(item);
            map.SetBlock(row, col, item);
            map.RePositionBlock(item);
            item.SetColorLayer(color);

            StateDatas.Instance.AbsolutePopBlocks.Add(item);
         }
      }

      async Task ToyPartyItemChangeAnimation(Block dst, float speedMulti)
      {
         var shot = Instantiate(m_toyPartyCannonShot);
         shot.GetComponent<TrailRenderer>().startColor = ColorManager.Instance.GetColor(dst.ColorLayer);

         var beg = m_toyCannonTrans.position;

         await new Simation(beg, dst.transform.position, shot.transform, m_toyPartyCannonShotSec / speedMulti).StartAsync();

         Destroy(shot);
      }

      BlockEntity GetNextToyPartyItem()
      {
         return m_toyPartyItems[Random.Range(0, m_toyPartyItems.Count)];
      }
   }
}