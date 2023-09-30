using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityCommon
{
	public class InspectorReadOnlyAttribute : PropertyAttribute
	{
		public readonly bool runtimeOnly;

		public InspectorReadOnlyAttribute(bool runtimeOnly = false)
		{
			this.runtimeOnly = runtimeOnly;
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(InspectorReadOnlyAttribute), true)]
	class ReadOnlyAttributeDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
            bool temp = GUI.enabled;

            if (((InspectorReadOnlyAttribute)attribute).runtimeOnly)
            {
                GUI.enabled = !Application.isPlaying;
            }
            else
            {
                GUI.enabled = false;
            }

			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = temp;
		}
	}
#endif
}