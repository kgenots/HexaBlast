using UnityEngine;
using UnityCommon;

namespace HexaBlast
{
   public abstract class Manager<TDerived> : MonoBehaviourSingleton<TDerived>, IManager where TDerived : Manager<TDerived>
   {
      private void Reset()
      {
         BootManager.Instance.VerifyRegistration(this);
      }

      public virtual void OnStartManager() { }
      public virtual void OnFinalManager() { }
   }

   interface IManager
   {
      void OnStartManager();
      void OnFinalManager();
   }
}