using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace HexaBlast.Scenario.StageScene
{
   class PopState : State
   {
      [SerializeField] float m_itemMergeAnimationSec = 0.5f;
      [SerializeField] int m_popScore = 20;

      public override void Initialize()
      {
      }

      public override void BeginState()
      {
         StartPopAsync();
      }

      async void StartPopAsync()
      {
         bool isPoped = await TryPopAllAsync1();

         if (isPoped)
         {
            StateManager.Instance.ChangeState(StateType.Drop);
         }
         else if (!isPoped)
         {
            StateManager.Instance.ChangeState(StateType.Clear);
         }
      }

      async Task<bool> TryPopAllAsync1()
      {
         var datas = StateDatas.Instance;
         Dictionary<Block, int> hpDeltaSet = new Dictionary<Block, int>();
         List<Block> items = new List<Block>();
         bool isPoped = false;

         foreach (Block b in datas.AbsolutePopBlocks)
         {
            hpDeltaSet.Add(b, -b.HP);
         }
         datas.AbsolutePopBlocks.Clear();

         HashSet<Block> swapSet = datas.SwapPopChecks;
         HashSet<Block> dropSet = datas.DropPopChecks;
         List<ItemCreateInfo> itemInfo0 = new List<ItemCreateInfo>();
         FigureCheck(swapSet, hpDeltaSet, itemInfo0);
         FigureCheck(dropSet, hpDeltaSet, itemInfo0);
         swapSet.Clear();
         dropSet.Clear();

         ChooseItem(itemInfo0, out var itemInfo1);
         var itemInfo2 = new List<ItemCreateInfo>(itemInfo1);

         HashSet<Block> frameVisit = new HashSet<Block>();
         HashSet<Block> popSet = new HashSet<Block>();
         bool affectChainLoop = true;
         while (affectChainLoop)
         {
            bool itemChainLoop = true;
            while (itemChainLoop)
            {
               foreach (var pair in hpDeltaSet)
               {
                  frameVisit.Add(pair.Key);
               }

               DecreaseHps(hpDeltaSet, out var popBlock);
               isPoped |= hpDeltaSet.Count > 0;
               hpDeltaSet.Clear();
               foreach (var b in popBlock)
               {
                  popSet.Add(b);
               }

               CollectItemRange(popBlock, items, hpDeltaSet);
               itemChainLoop = hpDeltaSet.Count > 0;
            }

            await StartItemsAnimationAsync(items);
            items.Clear();

            await StartItemGenerateAnimationAsync(itemInfo1);
            itemInfo1.Clear();

            await StartPopAnimationAsync(popSet);

            PopBlocks(popSet, frameVisit, hpDeltaSet);
            popSet.Clear();
            affectChainLoop = hpDeltaSet.Count > 0;
         }

         GenerateItems(itemInfo2);

         return isPoped;
      }

      void FigureCheck(HashSet<Block> srcCheckSet, Dictionary<Block, int> dstSet, List<ItemCreateInfo> itemGenInfo)
      {
         HashSet<Block> hashSet = new HashSet<Block>();

         foreach (Block block in srcCheckSet)
         {
            bool canpop = FigureChecker.Instance.FigureCheck(block, hashSet, itemGenInfo);
         }

         foreach (Block b in hashSet)
         {
            if (!b) continue;

            if (!dstSet.ContainsKey(b))
            {
               dstSet.Add(b, -1);
            }
            else
            {
               dstSet[b] -= 1;
            }
         }
      }

      void DecreaseHps(Dictionary<Block, int> hpDeltaSet, out List<Block> poped)
      {
         poped = new List<Block>();

         foreach (var pair in hpDeltaSet)
         {
            pair.Key.SetHP(pair.Key.HP + pair.Value);

            if (pair.Key.HP <= 0)
            {
               poped.Add(pair.Key);
            }
         }
      }

      void CollectItemRange(List<Block> src, List<Block> usedItems, Dictionary<Block, int> hpDeltaSet)
      {
         List<Block> collected = new List<Block>();

         foreach (Block block in src)
         {
            if (!block.Entity.IsItem) continue;
            if (usedItems.Contains(block)) continue;

            usedItems.Add(block);

            var bhv = block.GetComponent<ItemBehaviour>();
            bhv.CollectRange(block, collected);

            foreach (Block b in collected)
            {
               if (b != block)
               {
                  if (hpDeltaSet.ContainsKey(b))
                     hpDeltaSet[b] -= 1;
                  else
                     hpDeltaSet.Add(b, -1);
               }
            }
         }
      }

      async Task StartItemsAnimationAsync(List<Block> items)
      {
         if (StateDatas.Instance.AsyncronouseItemAnimation)
         {
            List<Task> animations = new List<Task>();

            foreach (Block b in items)
            {
               var task = b.GetComponent<ItemBehaviour>()?.StartAnimation();
               animations.Add(task);
            }

            await Task.WhenAll(animations);
         }
         else
         {
            foreach (Block b in items)
            {
               var task = b.GetComponent<ItemBehaviour>()?.StartAnimation();
               await task;
            }
         }
      }

      async Task StartPopAnimationAsync(HashSet<Block> set)
      {
         List<Task> animations = new List<Task>();

         foreach (var b in set)
         {
            if (!b) continue;

            var task = b.GetComponent<BlockPopAnimation>()?.StartAnimationAsync();
            if (task != null)
            {
               animations.Add(task);
            }
         }

         await Task.WhenAll(animations);
      }

      void ChooseItem(List<ItemCreateInfo> srcList, out List<ItemCreateInfo> dstList)
      {
         HashSet<Block> visited = new HashSet<Block>();
         dstList = new List<ItemCreateInfo>();
         var map = BlockMap.Instance;

         foreach (var info in srcList)
         {
            var srcBlock = map.GetBlock(info.Row, info.Col);
            if (visited.Contains(srcBlock)) continue;

            bool isOk = true;
            foreach (var block in info.Mergeds)
            {
               if (visited.Contains(block))
               {
                  isOk = false;
                  break;
               }
            }

            if (isOk)
            {
               visited.Add(srcBlock);
               foreach (var block in info.Mergeds)
               {
                  visited.Add(block);
               }

               dstList.Add(info);
            }
         }
      }

      async Task StartItemGenerateAnimationAsync(List<ItemCreateInfo> infos)
      {
         var map = BlockMap.Instance;
         var animations = new List<Task>();

         foreach (var info in infos)
         {
            if (info.Mergeds == null) continue;

            Vector3 dstPos = map.IndexToWorldPos(info.Row, info.Col);

            foreach (var block in info.Mergeds)
            {
               var begPos = block.transform.position;
               var task = new Simation(begPos, dstPos, block.transform, Simation.EaseInOutCurve, m_itemMergeAnimationSec).StartAsync();

               animations.Add(task);
            }
         }

         await Task.WhenAll(animations);
      }


      void PopBlocks(HashSet<Block> blocks, HashSet<Block> frameVisit, Dictionary<Block, int> hpDelta)
      {
         foreach (Block b in blocks)
         {
            if (!b) continue;

            PopSingle(b, frameVisit, hpDelta);
         }
      }

      void PopSingle(Block b, HashSet<Block> frameVisit, Dictionary<Block, int> hpDelta)
      {
         var map = BlockMap.Instance;
         int r = b.Row;
         int c = b.Col;

         if (b.Entity.CanAffectOther)
         {
            foreach (var (rd, cd) in HexaDirections.GetDelta(c))
            {
               int nr = r + rd;
               int nc = c + cd;
               if (map.IsIndexOutOfRange(nr, nc)) continue;
               if (map.IsIndexNotEnabled(nr, nc)) continue;
               var nb = map.GetBlock(nr, nc);
               if (nb == null) continue;

               if (nb.Entity.CanAffected)
               {
                  if (frameVisit.Add(nb))
                  {
                     if (hpDelta.ContainsKey(nb))
                        hpDelta[nb] -= 1;
                     else
                        hpDelta.Add(nb, -1);
                  }
               }
            }
         }

         var socket = map.GetSocket(r, c);
         if (socket.Entity.CanAffectedByAbove)
         {
            socket.SetHP(socket.HP - 1);
         }

         Stage.Instance.AddScore(m_popScore);

         BlockMap.Instance.RemoveBlock(b);
         BlockFactory.Instance.RemoveBlock(ref b);
      }

      void GenerateItems(List<ItemCreateInfo> itemsGens)
      {
         if (itemsGens.Count <= 0) return;

         var map = BlockMap.Instance;
         foreach (var info in itemsGens)
         {
            if (!info.ItemEntity) continue;

            var item = BlockFactory.Instance.GetBlock(info.ItemEntity);
            map.SetBlock(info.Row, info.Col, item);
            map.AddChildBlock(item);
            map.RePositionBlock(item);

            item.SetColorLayer(info.ColorLayer);
         }
      }
   }
}