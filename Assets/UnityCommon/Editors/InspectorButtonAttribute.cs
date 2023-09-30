using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityCommon
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class InspectorButtonAttribute : PropertyAttribute
    {
        public readonly string label;
        public readonly string methodName;
        public readonly float buttonWidth;
        public readonly string[] pars;

        public InspectorButtonAttribute(string MethodName, string label = null, int Width = 200, params string[] Pars)
        {
            methodName = MethodName;
            this.label = label != null ? label : MethodName;
            buttonWidth = Width;
            pars = Pars.Length <= 0 ? null : Pars;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InspectorButtonAttribute))]
    class ButtonAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GUI.skin.button.CalcHeight(label, (attribute as InspectorButtonAttribute).buttonWidth);
        }

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            var att = attribute as InspectorButtonAttribute;
			var labelText = att.label;
            var methodName = att.methodName;
            var btnWidth = att.buttonWidth;
            var pars = att.pars;

            var viewWidth = EditorGUIUtility.currentViewWidth;
            var x = viewWidth / 2 - btnWidth / 2;
            var y = position.y;
            var btnHeight = position.height;
            Rect buttonRect = new Rect(x, y, btnWidth, btnHeight);
            
            if (GUI.Button(buttonRect, labelText))
            {
                object target = prop.serializedObject.targetObject;
                Type classType = target.GetType();
                Type[] parTypes = new Type[0];
                object[] parValues = new object[0];
                MethodInfo methodInfo = null;
                
                if (pars != null)
                {
                    parTypes = new Type[pars.Length];
                    parValues = new object[pars.Length];

                    for (int i = 0; i < pars.Length; ++i)
                    {
                        var field = classType.GetField(pars[i],
                            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        parTypes[i] = field.FieldType;
                        parValues[i] = field.GetValue(target);
                    }
                }

                methodInfo = classType.GetMethod(methodName,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly, 
                    null, parTypes, null);
                
                if (methodInfo != null)
                {
                    methodInfo.Invoke(target, parValues);
                }
                else
                {
                    string partypes = "{ ";
                    string parvalues = "{ ";

                    foreach (var v in parTypes)
                        partypes += v.ToString() + " ";

                    foreach (var v in parValues)
                        parvalues += v.ToString() + " ";

                    partypes += "}";
                    parvalues += "}";

                    Debug.LogError(string.Format("InspectorButton: Unable to find method name={0} in type={1}, parTypes={2}, parValues={3}", methodName, classType, partypes, parvalues));
                }
            }
        }
    }
#endif
}