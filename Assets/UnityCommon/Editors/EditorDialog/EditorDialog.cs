﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityCommon
{
    public interface IEditorDialog
    {
        string TitleText { get; set; }
        string SubmitText { get; set; }
        string CancelText { get; set; }
        Action OnSubmit { get; set; }
        Action OnCancel { get; set; }
        bool IsSubmitted { get; }
        bool IsCanceled { get; }
        bool IsModal { get; set; }
        EditorButton SubmitButton{ get; }
        EditorButton CancelButton{ get; }
        List<EditorButton> Buttons { get; }

        void Show();
        void Show(Action onSubmit);
        void Show(Action onSubmit, Action onCancel);
        void Show(bool showSubmit, Action onSubmit, bool showCancel, Action onCancel);
        void Show(EditorButton submit);
        void Show(EditorButton submit, EditorButton cancel);
        void Show(EditorButton submit, EditorButton cancel, params EditorButton[] buttons);
    }

    public abstract class EditorDialog : IEditorDialog
    {
        static EditorButton DefaultSubmit = new EditorButton("Submit", true, false, null);
        static EditorButton DefaultCancel = new EditorButton("Cancel", true, false, null);
        const int IndexOfSubmitButton = 0;
        const int IndexOfCancelButton = 1;
        const string DefaultTitle = "Dialog";

        List<EditorButton> m_buttons;
        EditorDialogWindow m_window;
        bool m_isSubmited;
        bool m_isCanceled;
        bool m_isModal;

        public string TitleText
        {
            get => m_window.titleContent.text;
            set => m_window.titleContent.text = value;
        }
        public string SubmitText
        {
            get => m_buttons[IndexOfSubmitButton].Text;
            set => m_buttons[IndexOfSubmitButton].Text = value;
        }
        public string CancelText
        {
            get => m_buttons[IndexOfCancelButton].Text;
            set => m_buttons[IndexOfCancelButton].Text = value;
        }
        public Action OnSubmit
        {
            get => m_buttons[IndexOfSubmitButton].Callback;
            set => m_buttons[IndexOfSubmitButton].Callback = value;
        }
        public Action OnCancel
        {
            get => m_buttons[IndexOfCancelButton].Callback;
            set => m_buttons[IndexOfCancelButton].Callback = value;
        }
        public bool IsSubmitted
        {
            get => m_isSubmited;
        }
        public bool IsModal
        {
            get => m_isModal;
            set => m_isModal = value;
        }
        public bool IsCanceled
        {
            get => m_isCanceled;
        }
        public EditorButton SubmitButton
        {
            get => m_buttons[IndexOfSubmitButton];
        }
        public EditorButton CancelButton
        {
            get => m_buttons[IndexOfCancelButton];
        }
        public List<EditorButton> Buttons
        {
            get => m_buttons;
        }


        public EditorDialog() : this(DefaultTitle)
        {

        }

        public EditorDialog(string title)
        {
            m_window = ScriptableObject.CreateInstance<EditorDialogWindow>();
            m_window.titleContent.text = title;
            m_window.SetConfig(this);
            m_buttons = new List<EditorButton>();
            m_buttons.Add(new EditorButton(DefaultSubmit));
            m_buttons.Add(new EditorButton(DefaultCancel));
        }

        public void Show()
        {
            if (m_isModal)
            {
                m_window.ShowModal();
            }
            else
            {
                m_window.Show();
            }
        }

        public void Show(Action onSubmit)
        {
            Show(true, onSubmit, false, null);
        }

        public void Show(Action onSubmit, Action onCancel)
        {
            Show(true, onSubmit, true, onCancel);
        }

        public void Show(bool showSubmit, Action onSubmit, bool showCancel, Action onCancel)
        {
            m_buttons[IndexOfSubmitButton].Hidden = !showSubmit;
            m_buttons[IndexOfSubmitButton].Callback = onSubmit;
            m_buttons[IndexOfCancelButton].Hidden = !showCancel;
            m_buttons[IndexOfCancelButton].Callback = onSubmit;
            Show();
        }

        public void Show(EditorButton submit)
        {
            Show(submit, null, null);
        }

        public void Show(EditorButton submit, EditorButton cancel)
        {
            Show(submit, cancel, null);
        }

        public void Show(EditorButton submit, EditorButton cancel, params EditorButton[] buttons)
        {
            m_buttons[IndexOfSubmitButton] = submit;
            m_buttons[IndexOfCancelButton] = cancel;

            if (buttons != null)
            {
                m_buttons.AddRange(buttons);
            }

            Show();
        }

        public void ShowModal()
        {
            m_isModal = true;
            Show();
        }

        protected void OnGUIButtons()
        {
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();

            for (int i = 0; i< m_buttons.Count; ++i)
            {
                var btn = m_buttons[i];

                if (btn != null && !btn.Hidden)
                {
                    if (GUILayout.Button(btn.Text))
                    {
                        if (i == IndexOfSubmitButton)
                        {
                            OnPreSubmit();
                            m_isSubmited = true;
                        }
                        else if (i == IndexOfCancelButton)
                        {
                            OnPreCancel();
                            m_isCanceled = true;
                        }

                        try
                        {
                            btn.Callback?.Invoke();
                        }
                        catch(Exception e)
                        {
                            m_window.Close();
                            throw e;
                        }

                        if (btn.CloseWindow)
                        {
                            m_window.Close();
                        }

                        if (i == IndexOfSubmitButton)
                        {
                            OnPostSubmit();
                        }
                        else if (i == IndexOfCancelButton)
                        {
                            OnPostCancel();
                        }
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        protected abstract void OnGUIContext();
        protected virtual void OnPreSubmit() { }
        protected virtual void OnPreCancel() { }
        protected virtual void OnPostSubmit() { }
        protected virtual void OnPostCancel() { }
        
        class EditorDialogWindow : EditorWindow
        {
            [SerializeField] bool m_isDisabled;

            bool m_isPositionInitialized;
            EditorDialog m_dialog;
            Vector2 m_scrollPos;

            private void OnDisable()
            {
                m_isDisabled = true;
            }

            protected void OnGUI()
            {
                if (m_isDisabled)
                {
                    Close();
                    return;
                }

                if (!m_isPositionInitialized)
                {
                    m_isPositionInitialized = true;

                    var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                    mousePos.x -= position.width;
                    position = new Rect(mousePos, position.size);
                }

                EditorGUILayout.BeginScrollView(m_scrollPos);

                m_dialog.OnGUIContext();

                m_dialog.OnGUIButtons();

                EditorGUILayout.EndScrollView();
            }

            public void SetConfig(EditorDialog dialog)
            {
                m_dialog = dialog;
            }
        }
    }
    
    public class EditorButton
    {
        public string Text;
        public bool Hidden;
        public bool CloseWindow;
        public Action Callback;

        public EditorButton()
            : this("", false, false, null)
        {

        }

        public EditorButton(string text, Action callback)
            : this(text, false, false, callback)
        {

        }

        public EditorButton(string text, bool closeWindow, Action callback)
            : this(text, closeWindow, false, callback)
        {

        }

        public EditorButton(string text, bool closeWindow, bool hidden, Action callback)
        {
            Text = text;
            CloseWindow = closeWindow;
            Hidden = hidden;
            Callback = callback;
        }

        public EditorButton(EditorButton other)
        {
            Text = other.Text;
            Hidden = other.Hidden;
            Callback = other.Callback;
            CloseWindow = other.CloseWindow;
        }
    }

}
#endif