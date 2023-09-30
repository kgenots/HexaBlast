using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HexaBlast
{
   [ExecuteAlways]
   class TaskWatcher : UnityCommon.MonoBehaviourSingleton<TaskWatcher>
   {
      LinkedList<Task> m_list = new LinkedList<Task>();

      private void Update()
      {
         if (m_list.Count < 0) return;
         var node = m_list.First;

         while (node != null)
         {
            var task = node.Value;

            if (task.IsFaulted)
            {
               Debug.LogException(task.Exception);
            }

            var prev = node;
            node = node.Next;

            if (task.IsCompleted)
            {
               m_list.Remove(prev);
            }
         }
      }

      public void WatchTask(Task task)
      {
         m_list.AddLast(task);
      }
   }

   static class TaskEx
   {
      public static void Watch(this Task src)
      {
         TaskWatcher.Instance.WatchTask(src);
      }
   }
}