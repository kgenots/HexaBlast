#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UnityCommon
{
   public abstract class ToolbarItem : ScriptableObject
   {
      [SerializeField] Toolbar m_toolbar;
      [SerializeField] bool m_isSelected;

      public abstract new string name { get; }

      public bool IsSelected
      {
         get => m_isSelected;
         internal set => m_isSelected = value;
      }

      public void SetToolbar(Toolbar src)
      {
         m_toolbar = src;
      }

      public Toolbar Toolbar
      {
         get => m_toolbar;
      }

      public abstract void OnGUI();

      public virtual void OnSelected()
      {
      }

      public virtual void OnDeselected()
      {
      }
   }
}
#endif