#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace UnityCommon
{
   public class ObjectSingleton<TDerived> where TDerived : class, new()
   {
      protected static object m_mutex;
      protected static TDerived m_instance;

      static ObjectSingleton()
      {
         m_mutex = new object();
      }

      public static TDerived Instance
      {
         get
         {
            if (m_instance == null)
            {
               lock (m_mutex)
               {
                  if (m_instance == null)
                  {
                     m_instance = new TDerived();
                  }
               }
            }

            return m_instance;
         }
      }
   }
}