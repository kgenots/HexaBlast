#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityCommon;
using System.Collections.Generic;
using System;

namespace HexaBlast
{
   [ExecuteAlways]
   partial class BootManager : MonoBehaviourSingleton<BootManager>
   {
      [Serializable]
      class ManagerToken
      {
         public MonoBehaviour Mono;
         public string LastName; // For Debug
         public int StartOrder;
         public int FinalOrder;
      }

      [SerializeField] List<ManagerToken> m_managers;

      void Reset()
      {
         if (m_managers == null) m_managers = new List<ManagerToken>();
         foreach (var cmp in GetComponentsInChildren<IManager>())
         {
            VerifyRegistration(cmp);
         }
      }

      private void OnEnable()
      {
         if (m_managers == null) m_managers = new List<ManagerToken>();

         Validation();

         SortAsStart(true);

         foreach (var token in m_managers)
         {
            var man = (IManager)token.Mono;

            man.OnStartManager();
         }
      }

      private void OnDisable()
      {
         Validation();

         SortAsFinal(true);

         foreach (var token in m_managers)
         {
            var man = (IManager)token.Mono;

            man.OnFinalManager();
         }
      }

      public void VerifyRegistration(IManager src)
      {
         var mono = (MonoBehaviour)src;

         for (int i = 0; i < m_managers.Count; ++i)
         {
            if (m_managers[i].Mono == mono)
            {
               return;
            }
         }

         m_managers.Add(new ManagerToken()
         {
            Mono = mono,
         });

         mono.transform.SetParent(transform);
         mono.name = src.GetType().Name;

#if UNITY_EDITOR
         EditorUtility.SetDirty(this);
         EditorGUIUtility.PingObject(mono);
#endif
      }

      void Validation()
      {
         for (int i = 0; i < m_managers.Count; ++i)
         {
            if (!m_managers[i].Mono)
            {
               Debug.Log("Invalid Manager Removed : " + m_managers[i].LastName);
               m_managers.RemoveAt(i);
               --i;
            }

            // Record Name For Debug
            m_managers[i].LastName = m_managers[i].Mono.name;
         }
      }

      void SortAsName(bool isAscent)
      {
         if (isAscent)
         {
            m_managers.Sort((x, y) =>
            {
               return string.Compare(x.Mono.name, y.Mono.name);
            });
         }
         else
         {
            m_managers.Sort((x, y) =>
            {
               return string.Compare(y.Mono.name, x.Mono.name);
            });
         }
      }

      void SortAsStart(bool isAscent)
      {
         if (isAscent)
         {
            m_managers.Sort((x, y) =>
            {
               return x.StartOrder - y.StartOrder;
            });
         }
         else
         {
            m_managers.Sort((x, y) =>
            {
               return y.StartOrder - x.StartOrder;
            });
         }
      }

      void SortAsFinal(bool isAscent)
      {
         if (isAscent)
         {
            m_managers.Sort((x, y) =>
            {
               return x.FinalOrder - y.FinalOrder;
            });
         }
         else
         {
            m_managers.Sort((x, y) =>
            {
               return y.FinalOrder - x.FinalOrder;
            });
         }
      }

      class StartComparer : IComparer<ManagerToken>
      {
         public int Compare(ManagerToken x, ManagerToken y)
         {
            return x.StartOrder - y.StartOrder;
         }
      }

      class FinalComparer : IComparer<ManagerToken>
      {
         public int Compare(ManagerToken x, ManagerToken y)
         {
            return x.FinalOrder - y.FinalOrder;
         }
      }
   }
}