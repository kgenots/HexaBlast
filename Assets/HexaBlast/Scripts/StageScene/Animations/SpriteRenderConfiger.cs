using System.Threading.Tasks;
using UnityEngine;

namespace HexaBlast
{
   abstract class SpriteRenderConfiger : MonoBehaviour
   {
      public abstract void EnableConfig();
      public abstract void DisableConfig();
   }
}