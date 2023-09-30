#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace UnityCommon
{
    public class EditorDialogVector3 : EditorDialog
    {
        public Vector3 Value;
        public string Label;

        public static void Show(string label, Vector3 value, Action<EditorDialogVector3> submit)
        {
            var i = new EditorDialogVector3();
            i.Value = value;
            i.Label = label;
            i.OnSubmit = ()=>submit(i);

            i.Show();
        }

        protected override void OnGUIContext()
        {
            Value = EditorGUILayout.Vector3Field(Label, Value);
        }
    }
}
#endif