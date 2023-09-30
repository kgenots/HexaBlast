#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace UnityCommon
{
   public class MonoBehaviourSingleton<TDerived> : MonoBehaviour where TDerived : MonoBehaviour
   {
      protected static object m_mutex;
      protected static TDerived m_instance;

      static MonoBehaviourSingleton()
      {
         m_mutex = new object();
      }

      public static TDerived Instance
      {
         get
         {
            if (!m_instance)
            {
               lock (m_mutex)
               {
                  if (!m_instance)
                  {
                     FindOrCreate();
                  }
               }
            }

            return m_instance;
         }
      }

      static void FindOrCreate()
      {
         m_instance = FindObjectOfType<TDerived>() as TDerived;

         if (!m_instance)
         {
            GameObject o = new GameObject(typeof(TDerived).Name);
            m_instance = o.AddComponent<TDerived>();
         }
      }
   }
}