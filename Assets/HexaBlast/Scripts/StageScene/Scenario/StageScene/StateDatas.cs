using System.Collections.Generic;
using UnityEngine;
using HexaBlast.Scenario.StageScene;

namespace HexaBlast
{
   class StateDatas : UnityCommon.ObjectSingleton<StateDatas>
   {
      public HashSet<Block> DropPopChecks = new HashSet<Block>();
      public HashSet<Block> SwapPopChecks = new HashSet<Block>();
      public HashSet<Block> AbsolutePopBlocks = new HashSet<Block>();
      public bool AsyncronouseItemAnimation = false;
   }
}