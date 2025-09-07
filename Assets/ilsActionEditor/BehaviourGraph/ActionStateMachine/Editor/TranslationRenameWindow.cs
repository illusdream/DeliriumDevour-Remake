using System;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json.Serialization;
using UnityEditor;
using UnityEngine;
using XNode;
using Action = System.Action;

namespace ilsActionEditor.Editor
{
    public class TranslationRenameWindow : EditorWindow
    {
        private const string inputControlName = "translationNameInput";
        
        public static TranslationRenameWindow current { get; private set; }

        public AFSMStateTranslation target;
        public string input;
        
        private bool firstFrame = true;
        private System.Action<string> callback;
        public static TranslationRenameWindow Show(AFSMStateTranslation target,System.Action<string> callback = null, float width = 200)
        {
            TranslationRenameWindow window = (TranslationRenameWindow)EditorWindow.GetWindow(typeof(TranslationRenameWindow),true,$"Rename Translation: {target.Name}");
            if (current )
            {
                current.Close();
            }
            current = window;
            window.target = target;
            window.input = target.Name;
            window.minSize = new Vector2(100, 44);
            window.position = new Rect(0, 0, width, 44);
            window.callback = callback;
            window.UpdatePositionToMouse();
            return window;
        }

        private void UpdatePositionToMouse()
        {
            if (Event.current == null)
            {
                return;
            }
            Vector3 mousePoint = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            Rect pos = position;
            pos.x = mousePoint.x - position.width * 0.5f;
            pos.y = mousePoint.y - 10;
            position = pos;
        }
        private void OnLostFocus() {
            // Make the popup close on lose focus
            Close();
        }

        private void OnGUI()
        {
            if (firstFrame)
            {
                UpdatePositionToMouse();
                firstFrame = false;
            }

            GUI.SetNextControlName(inputControlName);
            input = EditorGUILayout.TextField(input);
            EditorGUI.FocusTextInControl(inputControlName);
            Event e = Event.current;
            // If input is empty, revert name to default instead
            if (input == null || input.Trim() == "")
            {
                if (GUILayout.Button("Revert to default") || (e.isKey && e.keyCode == KeyCode.Return))
                {

                    target.Name = target.IO switch
                    {
                        NodePort.IO.Input => (target.Node as AFSMStateNode).EntriesCount.ToString(),
                        NodePort.IO.Output => (target.Node as AFSMStateNode).Exits.ToString(),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    Close();
                }
            }
            // Rename asset to input text
            else
            {
                if (GUILayout.Button("Apply") || (e.isKey && e.keyCode == KeyCode.Return))
                {
                    if (target.Node is AFSMStateNode node)
                    {
                        switch (target.IO)
                        {
                            case NodePort.IO.Input:
                                if (node.Entries.Any(t =>t.Name == input))
                                {
                                    Close();
                                    EditorUtility.DisplayDialog("已有相同名字的转换", "已有相同名字的转换", "ok");
                                    return;
                                }
                                if (input.All(char.IsDigit))
                                {
                                    Close();
                                    EditorUtility.DisplayDialog("不能由纯数字构成名字", "已有相同名字的转换", "ok");
                                    return;
                                }
                                target.Rename(input) ;
                                callback?.Invoke(input);
                                Close();
                                break;
                            case NodePort.IO.Output:
                                if (node.Exits.Any(t =>t.Name == input))
                                {
                                    Close();
                                    EditorUtility.DisplayDialog("已有相同名字的转换", "已有相同名字的转换", "ok");
                                    return;
                                }
                                if (input.All(char.IsDigit))
                                {
                                    Close();
                                    EditorUtility.DisplayDialog("不能由纯数字构成名字", "已有相同名字的转换", "ok");
                                    return;
                                }
                                target.Rename(input) ;
                                callback?.Invoke(input);
                                Close();
                                break;
                        }
                    }

                }
            }

            if (e.isKey && e.keyCode == KeyCode.Escape)
            {
                Close();
            }
        }

        private void OnDestroy()
        {
            EditorGUIUtility.editingTextField = false;
        }
    }
}