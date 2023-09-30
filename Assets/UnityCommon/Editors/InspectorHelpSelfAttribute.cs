using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityCommon
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class InspectorHelpSelfAttribute : PropertyAttribute
    {
        public string Text { get; }
        public MessageType Type { get; }
        public string Condition { get; set; }

        public InspectorHelpSelfAttribute(string text, MessageType type = MessageType.None)
        {
            Text = text;
            Type = type;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InspectorHelpSelfAttribute))]
    class InspectorHelpSelfAttributeDrawer : PropertyDrawer
    {
        static readonly RectOffset padding = new RectOffset(8, 8, 8, 8);
        static readonly RectOffset margin = new RectOffset(5, 5, 5, 3);

        float helpHeight;

        public InspectorHelpSelfAttribute Attribute => (InspectorHelpSelfAttribute)attribute;
        
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            var helpAtt = Attribute;

            if (!ConditionCheck(prop.serializedObject.targetObject, helpAtt.Condition))
            {
                return 0;
            }

            var content = new GUIContent(helpAtt.Text);
            var style = GUI.skin.GetStyle("helpbox");

            style.padding = padding;
            style.margin = margin;
            style.alignment = TextAnchor.MiddleLeft;

            helpHeight = style.CalcHeight(content, EditorGUIUtility.currentViewWidth - margin.horizontal);

            return helpHeight + margin.vertical;
        }

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            var helpAtt = Attribute;

            if (!ConditionCheck(prop.serializedObject.targetObject, helpAtt.Condition))
            {
                return;
            }

            position.x = margin.left;
            position.y += margin.top;
            position.width = EditorGUIUtility.currentViewWidth - margin.horizontal;
            position.height = helpHeight;

            EditorGUI.HelpBox(position, helpAtt.Text, helpAtt.Type);
        }

        bool ConditionCheck(object target, string name)
        {
            if (name == null) return true;

            Type classType = target.GetType();
            var field = classType.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var method = classType.GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, binder: null, types: new Type[] { }, modifiers: null);
            
            if (field != null)
            {
                if (field.FieldType == typeof(bool))
                {
                    return (bool)field.GetValue(target);
                }
            }
            else if (method != null)
            {
                if (method.ReturnType == typeof(bool))
                {
                    return (bool)method.Invoke(target, null);
                }
            }

            throw new Exception("Finding Condition " + name + " Fail");
        }
    }
#endif
}