﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityCommon
{
    public class EditorDialogMessageSubmit : EditorDialog
    {
        public string Message;

        public static void ShowOnCancel(string message, Action cancel, bool isModal = false)
        {
            Show(message, null, cancel, isModal);
        }

        public static void ShowOnSubmit(string message, Action submit, bool isModal = false)
        {
            Show(message, submit, null, isModal);
        }

        public static void Show(string message, Action submit, Action cancel, bool isModal = false)
        {
            var dialog = new EditorDialogMessageSubmit();
            dialog.Message = message;
            dialog.OnSubmit = submit;
            dialog.OnCancel= cancel;
            dialog.IsModal = isModal;

            dialog.Show();
        }

        public EditorDialogMessageSubmit() : base("Message")
        {

        }

        protected override void OnGUIContext()
        {
            EditorGUILayout.LabelField(Message);
        }
    }
}
#endif