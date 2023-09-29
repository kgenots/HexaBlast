using UnityEngine;

namespace HexaBlast
{
   abstract class UIPanel : MonoBehaviour
   {
      public abstract void Open();

      public abstract void Close();

      public abstract bool IsActive { get; }
   }
}