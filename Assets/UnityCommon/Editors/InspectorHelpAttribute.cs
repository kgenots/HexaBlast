//------------------------------------------------------------------------------------------------------------------
using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityCommon
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class InspectorHelpAttribute : PropertyAttribute
    {
        public string Text { get; }

        public bool Below { get; }

        public MessageType Type { get; }

        public string Condition { get; set; }

        public InspectorHelpAttribute(string text, bool below = true, MessageType type = MessageType.None)
        {
            Text = text;
            Below = below;
            Type = type;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InspectorHelpAttribute))]
    class HelpAttributeDrawer : PropertyDrawer
    {
        readonly RectOffset padding = new RectOffset(8, 8, 8, 8);
        readonly RectOffset margin = new RectOffset(5, 5, 2, 5);
        const int minHeight = 30;

        float baseHeight = 0;
        float helpHeight = 0;

        public InspectorHelpAttribute Attribute => (InspectorHelpAttribute)attribute;

        protected T TryGetPropertyAttribute<T>() where T : PropertyAttribute
        {
            var attributes = fieldInfo.GetCustomAttributes(typeof(T), true);
            return attributes != null && attributes.Length > 0 ? (T)attributes[0] : null;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            var helpAttribute = Attribute;

            baseHeight = base.GetPropertyHeight(prop, label);

            if (!ConditionCheck(prop.serializedObject.targetObject, helpAttribute.Condition))
                return baseHeight;

            var content = new GUIContent(helpAttribute.Text);
            var style = new GUIStyle(GUI.skin.GetStyle("helpbox"));

            style.padding = padding;
            style.margin = margin;
            style.alignment = TextAnchor.MiddleLeft;

            helpHeight = style.CalcHeight(content, EditorGUIUtility.currentViewWidth - margin.horizontal);

            /*
            if (helpAttribute.type != MessageType.None && helpHeight < minHeight)
            {
                RectOffset iconPadding = new RectOffset(padding.left, padding.right, padding.top + 5, padding.bottom + 5);
                style.padding = iconPadding;
                helpHeight = style.CalcHeight(content, EditorGUIUtility.currentViewWidth - margin.horizontal);
            }
            */

            var multilineAtt = TryGetPropertyAttribute<MultilineAttribute>();
            if (multilineAtt != null && prop.propertyType == SerializedPropertyType.String)
            {
                baseHeight += multilineAtt.lines * EditorGUIUtility.singleLineHeight;
            }

            return baseHeight + helpHeight + margin.vertical;
        }

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            var helpAttribute = Attribute;

            if (!ConditionCheck(prop.serializedObject.targetObject, helpAttribute.Condition))
            {
                EditorGUI.PropertyField(position, prop);
                return;
            }

            var multiline = TryGetPropertyAttribute<MultilineAttribute>();
            var range = TryGetPropertyAttribute<RangeAttribute>();

            EditorGUI.BeginProperty(position, label, prop);

            var basePos = position;
            var helpPos = position;

            if (helpAttribute.Below)
            {
                basePos.height = baseHeight;
                helpPos.y += basePos.height + margin.top;
                helpPos.x = margin.left;
                helpPos.width = EditorGUIUtility.currentViewWidth - margin.horizontal;
                helpPos.height = helpHeight;
            }
            else
            {
                helpPos.height = helpHeight;
                helpPos.y += margin.top;
                helpPos.x = margin.left;
                helpPos.width = EditorGUIUtility.currentViewWidth - margin.horizontal;

                basePos.height = baseHeight;
                basePos.y += helpPos.height + margin.bottom;
            }

            if (helpAttribute.Below)
            {
                DrawBaseProperty(basePos, prop, label, range, multiline);

                EditorGUI.HelpBox(helpPos, helpAttribute.Text, helpAttribute.Type);
            }
            else
            {
                EditorGUI.HelpBox(helpPos, helpAttribute.Text, helpAttribute.Type);

                DrawBaseProperty(basePos, prop, label, range, multiline);
            }

            EditorGUI.EndProperty();
        }

        void DrawBaseProperty(Rect position, SerializedProperty prop, GUIContent label, RangeAttribute range, MultilineAttribute multiline)
        {
            if (range != null)
            {
                if (prop.propertyType == SerializedPropertyType.Float)
                {
                    EditorGUI.Slider(position, prop, range.min, range.max, label);
                }
                else if (prop.propertyType == SerializedPropertyType.Integer)
                {
                    EditorGUI.IntSlider(position, prop, (int)range.min, (int)range.max, label);
                }
                else
                {
                    EditorGUI.PropertyField(position, prop, label);
                }
            }
            else if (multiline != null)
            {
                if (prop.propertyType == SerializedPropertyType.String)
                {
                    var posLabel = position;
                    var posTextArea = position;

                    posLabel.height = GUI.skin.label.CalcHeight(label, EditorGUIUtility.currentViewWidth);
                    posTextArea.height = position.height - posLabel.height;
                    posTextArea.y = position.y + posLabel.height;

                    EditorGUI.LabelField(posLabel, label);
                    prop.stringValue = EditorGUI.TextArea(posTextArea, prop.stringValue);
                }
                else
                {
                    EditorGUI.PropertyField(position, prop, label);
                }
            }
            else
            {
                EditorGUI.PropertyField(position, prop, label);
            }
        }

        bool ConditionCheck(object target, string checkerReflectionName)
        {
            if (checkerReflectionName == null) return true;

            Type classType = target.GetType();
            var field = classType.GetField(checkerReflectionName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var method = classType.GetMethod(checkerReflectionName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, binder: null, types: new Type[] { }, modifiers: null);
            Debug.Log(field);
            Debug.Log(method);
            if (field != null)
            {
                Debug.Log(field.FieldType + "/" + typeof(bool));
                if (field.FieldType == typeof(bool))
                {
                    return (bool)field.GetValue(target);
                }
            }
            else if (method != null)
            {
                Debug.Log(method.ReturnType + "/" + typeof(bool));
                if (method.ReturnType == typeof(bool))
                {
                    return (bool)method.Invoke(target, null);
                }
            }

            throw new Exception("Finding Condition " + checkerReflectionName + " Fail");
        }

    }
#else
    public enum MessageType
    {
        None,
        Info,
        Warning,
        Error,
    }
#endif
}