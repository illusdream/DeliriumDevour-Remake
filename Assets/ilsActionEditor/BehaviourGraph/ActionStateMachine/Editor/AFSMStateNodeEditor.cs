using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Animancer.Editor;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace ilsActionEditor.Editor
{
  [CustomNodeEditor(typeof(AFSMStateNode))]
  public class AFSMStateNodeEditor : NodeEditor
  {
    private NodePort _entry;
    private NodePort _exit;
    AFSMStateNode _state;


    private Dictionary<string, Rect> _entryRects = new Dictionary<string, Rect>();
    private Dictionary<string, Rect> _exitRects = new Dictionary<string, Rect>();

    private Rect _enterRect;
    private Rect _exitRect;

    private Texture2D _opaqueTex;

    private Texture2D opaqueTex
    {
      get
      {
        if (_opaqueTex == null)
        {
          _opaqueTex = new Texture2D(1, 1);
          _opaqueTex.SetPixel(0, 0, Color.white); // 这里用灰色，完全不透明
          _opaqueTex.Apply();
        }

        return _opaqueTex;
      }
    }

    public override void OnCreate()
    {
      UpdateAllNodeWithTranlationAttribute();
      CleanAllNonTransaltionPorts();
      base.OnCreate();
    }

    public override void OnBodyGUI()
    {
      base.OnBodyGUI();
      _state = target as AFSMStateNode;
      var lastestRect = GUILayoutUtility.GetLastRect();

      var offest = 20;

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.BeginVertical(GUILayout.Width(GetWidth() / 2f - offest));
      Rect EntryRect = new Rect(0, lastestRect.yMax, GetWidth() / 2f - offest, 100);
      DrawEntriesTransition(EntryRect);
      EditorGUILayout.EndVertical();

      EditorGUILayout.BeginVertical(GUILayout.Width(GetWidth() / 2f - offest));
      Rect ExitRect = new Rect(GetWidth() / 2f - offest, lastestRect.yMax, GetWidth() / 2f - offest, 100);
      DrawExitsTransition(ExitRect);
      EditorGUILayout.EndVertical();

      EditorGUILayout.EndHorizontal();


    }

    private bool ShowAddEntryTransition;
    private string AddEntryTranslationSearchString = "";
    private Vector2 AddEntryTransitionPos;
    private Rect oldEntryIgnoreScrollWhellRect;
    private List<string> needEntryRemovePortTranslation = new List<string>();
    private void DrawEntriesTransition(Rect rect)
    {
      EditorGUILayout.BeginVertical(GUILayout.Width(rect.width), GUILayout.ExpandWidth(false), GUILayout.MaxWidth(rect.width), GUILayout.MinWidth(rect.width));
      GUILayout.Label("Entries Transition", EditorStyles.boldLabel);
      int i = 0;

      foreach (var child in objectTree.GetPropertyAtPath("fieldEntryTranslations").Children)
      {
        var instance = (child.ValueEntry.WeakSmartValue as AFSMStateTranslation);
        if (instance is null)
        {
          continue;
        }
        {
          EditorGUILayout.BeginVertical();

          if (instance.Name != null && _entryRects.TryGetValue(instance.Name, out var portRect))
          {
            var oldColor = GUI.color;
            GUI.color = i % 2 == 0 ? new Color(0.2f, 0.2f, 0.2f, 1f) : new Color(0.3f, 0.3f, 0.3f, 1f);
            GUI.DrawTexture(new Rect(portRect.x, portRect.y, portRect.width, portRect.height + 2), opaqueTex);
            GUI.color = oldColor;
          }

          i++;
          var _entry = target.GetInputPort("entry"+instance.Name);
          if (_entry != null)
          {
            NodeEditorGUILayout.PortField(new GUIContent(instance.Name), _entry);
          }


          child.Label = new GUIContent("");
          child.Draw();
          EditorGUILayout.EndVertical();
        }
        if (GUILayoutUtility.GetLastRect().x != 0)
        {
          _entryRects[instance.Name] = GUILayoutUtility.GetLastRect();
        }
      }
      

      
      foreach (var child in objectTree.GetPropertyAtPath("entries").Children)
      {
        var instance = (child.ValueEntry.WeakSmartValue as AFSMStateTranslation);
        if (instance is null)
        {
          continue;
        }
        {
          EditorGUILayout.BeginVertical();

          if (_entryRects.TryGetValue(instance.Name, out var portRect))
          {
            var oldColor = GUI.color;
            GUI.color = i % 2 == 0 ? new Color(0.2f, 0.2f, 0.2f, 1f) : new Color(0.3f, 0.3f, 0.3f, 1f);
            GUI.DrawTexture(new Rect(portRect.x, portRect.y, portRect.width, portRect.height + 2), opaqueTex);
            GUI.color = oldColor;
          }

          i++;
          var _entry = target.GetInputPort("entry"+instance.Name);
          if (_entry == null)
          {
            _entry = target.AddDynamicInput(typeof(AFSMStateTranslation), XNode.Node.ConnectionType.Override, XNode.Node.TypeConstraint.Inherited,
              "entry" + instance.Name);
          }

          NodeEditorGUILayout.PortField(new GUIContent(instance.Name), _entry);

          child.Label = new GUIContent("");
          child.Draw();
          EditorGUILayout.EndVertical();
        }
        if (GUILayoutUtility.GetLastRect().x != 0)
        {
          _entryRects[instance.Name] = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.ContextClick && Event.current.button == 1 && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
          {
            EditorUtility.DisplayPopupMenu(GUILayoutUtility.GetLastRect(), "111", null);
              GenericMenu menu = new GenericMenu();
              menu.AddItem(new GUIContent("Remove"),false, () =>
              {
                needEntryRemovePortTranslation.Add(instance.Name);
              });
              menu.AddItem(new GUIContent("Rename"),false, () =>
              {
                TranslationRenameWindow.Show(instance);
              });
              menu.ShowAsContext();
          }
        }
      }

      GUILayout.Space(10);
      
      GUI.color = Color.white;
      
     if (GUILayout.Button("Add Transition", EditorStyles.miniButton, GUILayout.Height(20)))
     {
       ShowAddEntryTransition = !ShowAddEntryTransition;
     }
     var lastestRect = GUILayoutUtility.GetLastRect();
     var nextRectSize = lastestRect.width !=1 ? lastestRect.width : rect.width;
      
     Rect addRect = new Rect(lastestRect.xMin, lastestRect.yMin, nextRectSize, ShowAddEntryTransition ? 220 : 30);
     GUI.color = Color.gray;
     if (!addRect.Contains(Event.current.mousePosition)&& Event.current.type == EventType.MouseDown && lastestRect.x != 0)
     {
       ShowAddEntryTransition = false;
     }
     lastestRect = GUILayoutUtility.GetLastRect();
     GUILayout.BeginHorizontal();
     GUILayout.FlexibleSpace();
     GUILayout.BeginVertical();
     if (ShowAddEntryTransition)
     {
       var searchRect = Rect.MinMaxRect(lastestRect.x+4, lastestRect.y, lastestRect.xMax - 4, lastestRect.y + 20 - 1);
       GUI.color = Color.black;
       GUI.Box(addRect, "");  
       GUI.color = Color.white;
#if UNITY_2022_3_OR_NEWER
       var searchFieldText = (GUIStyle)"ToolbarSearchTextField";
       var searchFieldCancel = (GUIStyle)"ToolbarSearchCancelButton";
#else
            var searchFieldText = (GUIStyle)"ToolbarSeachTextField";
            var searchFieldCancel = (GUIStyle)"ToolbarSeachCancelButton";
#endif
       
      AddEntryTranslationSearchString = EditorGUILayout.TextField(AddEntryTranslationSearchString, searchFieldText,GUILayout.Width(rect.width-8));
      var finalHeight =175;
       // 过滤组件
       var filteredTypes = _state.GetAllCanUseEntryTransitions()
         .Where(t =>
         {
           return t != null && t.Name.Contains(AddEntryTranslationSearchString);
         })
         .ToList();
               
       // 滚动列表
       EditorGUILayout.BeginVertical();
       AddEntryTransitionPos = EditorGUILayout.BeginScrollView(AddEntryTransitionPos,false,false,GUILayout.Width(addRect.width-8),GUILayout.Height(finalHeight));
       var style = new GUIStyle(EditorStyles.toolbarButton);
       style.alignment = TextAnchor.MiddleLeft;
       foreach (var type in filteredTypes)
       {
         if (GUILayout.Button(new GUIContent(type.Name), style,
               GUILayout.MaxWidth(addRect.width)))
         {
           _state.AddEntryTransition(type,_state);
           ShowAddEntryTransition = false;
         }
       }


       EditorGUILayout.EndScrollView();
       EditorGUILayout.EndVertical();
       var scrollRect = GUILayoutUtility.GetLastRect().x !=0 ? GUILayoutUtility.GetLastRect() : oldEntryIgnoreScrollWhellRect;
       
       if (scrollRect.Contains(Event.current.mousePosition))
       {
         NodeEditorWindow.shouldIgnoreMouseScroll = true;
       }

       oldEntryIgnoreScrollWhellRect = scrollRect;
     }
     GUILayout.EndVertical();
     GUILayout.EndHorizontal();
     GUI.color = Color.white;

      EditorGUILayout.EndVertical();

      _enterRect = GUILayoutUtility.GetLastRect();


      foreach (var t in needEntryRemovePortTranslation)
      {
        try
        {
          _state.RemoveEntryTransition(t);
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
          throw;
        }
      }
      needEntryRemovePortTranslation.Clear();
    }
    private bool ShowAddExitTransition;
    private string AddExitTranslationSearchString = "";
    private Vector2 AddExitTransitionPos;
    private Rect oldExitIgnoreScrollWhellRect;
    private List<string> needExitRemovePortTranslation = new List<string>();
    private void DrawExitsTransition(Rect rect)
    {
      EditorGUILayout.BeginVertical(GUILayout.Width(rect.width), GUILayout.ExpandWidth(false), GUILayout.MaxWidth(rect.width), GUILayout.MinWidth(rect.width));
      var newSystle = new GUIStyle(EditorStyles.boldLabel);
      newSystle.alignment = TextAnchor.MiddleRight;
      GUILayout.Label("Exits Transition", newSystle);
      int i = 0;

      foreach (var child in objectTree.GetPropertyAtPath("fieldExitTranslations").Children)
      {
        var instance = (child.ValueEntry.WeakSmartValue as AFSMStateTranslation);
        if (instance is null)
        {
          continue;
        }
        EditorGUILayout.BeginVertical();
        if (instance.Name != null && _exitRects.TryGetValue(instance.Name, out var portRect))
        {
          var oldColor = GUI.color;
          GUI.color = i % 2 == 0 ? new Color(0.2f, 0.2f, 0.2f, 1f) : new Color(0.3f, 0.3f, 0.3f, 1f);
          GUI.DrawTexture(new Rect(portRect.x, portRect.y, portRect.width, portRect.height + 2), opaqueTex);
          GUI.color = oldColor;
        }

        i++;

        var _exit = target.GetOutputPort("exit" + instance.Name);
        if (_exit != null)
        {
   
            NodeEditorGUILayout.PortField(new GUIContent(instance.Name), _exit);
        }



        child.Label = new GUIContent("");
        child.Draw();

        EditorGUILayout.EndVertical();
        if (GUILayoutUtility.GetLastRect().x != 0)
        {
          _exitRects[instance.Name] = GUILayoutUtility.GetLastRect();
        }
      }
      
      foreach (var child in objectTree.GetPropertyAtPath("exits").Children)
      {
        var instance = (child.ValueEntry.WeakSmartValue as AFSMStateTranslation);
        if (instance is null)
        {
          continue;
        }
        EditorGUILayout.BeginVertical();
        if (_exitRects.TryGetValue(instance.Name, out var portRect))
        {
          var oldColor = GUI.color;
          GUI.color = i % 2 == 0 ? new Color(0.2f, 0.2f, 0.2f, 1f) : new Color(0.3f, 0.3f, 0.3f, 1f);
          GUI.DrawTexture(new Rect(portRect.x, portRect.y, portRect.width, portRect.height + 2), opaqueTex);
          GUI.color = oldColor;
        }

        i++;
        var _exit = target.GetOutputPort("exit" + instance.Name);
        if (_exit == null)
        {
          _exit = target.AddDynamicOutput(typeof(AFSMStateTranslation), XNode.Node.ConnectionType.Override, XNode.Node.TypeConstraint.Inherited,
            "exit" + instance.Name);
        }

        NodeEditorGUILayout.PortField(new GUIContent(instance.Name), _exit);

        child.Label = new GUIContent("");
        child.Draw();

        EditorGUILayout.EndVertical();
        if (GUILayoutUtility.GetLastRect().x != 0)
        {
          _exitRects[instance.Name] = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.ContextClick && Event.current.button == 1 && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
          {
            EditorUtility.DisplayPopupMenu(GUILayoutUtility.GetLastRect(), "111", null);
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Remove"),false, () =>
            {
              needExitRemovePortTranslation.Add(instance.Name);
            });
            menu.AddItem(new GUIContent("Rename"),false, () =>
            {
              TranslationRenameWindow.Show(instance);
            });
            menu.ShowAsContext();
          }
        }
      }

     GUILayout.Space(10);
      
      GUI.color = Color.white;
      
     if (GUILayout.Button("Add Transition", EditorStyles.miniButton, GUILayout.Height(20)))
     {
       ShowAddExitTransition = !ShowAddExitTransition;
     }
     var lastestRect = GUILayoutUtility.GetLastRect();
     var nextRectSize = lastestRect.width !=1 ? lastestRect.width : rect.width;
      
     Rect addRect = new Rect(lastestRect.xMin, lastestRect.yMin, nextRectSize, ShowAddExitTransition ? 220 : 30);
     GUI.color = Color.gray;
     if (!addRect.Contains(Event.current.mousePosition)&& Event.current.type == EventType.MouseDown && lastestRect.x != 0)
     {
       ShowAddExitTransition = false;
     }
     lastestRect = GUILayoutUtility.GetLastRect();
     GUILayout.BeginHorizontal();
     GUILayout.FlexibleSpace();
     GUILayout.BeginVertical();
     if (ShowAddExitTransition)
     {
       var searchRect = Rect.MinMaxRect(lastestRect.x+4, lastestRect.y, lastestRect.xMax - 4, lastestRect.y + 20 - 1);
       GUI.color = Color.black;
       GUI.Box(addRect, "");  
       GUI.color = Color.white;
#if UNITY_2022_3_OR_NEWER
       var searchFieldText = (GUIStyle)"ToolbarSearchTextField";
       var searchFieldCancel = (GUIStyle)"ToolbarSearchCancelButton";
#else
            var searchFieldText = (GUIStyle)"ToolbarSeachTextField";
            var searchFieldCancel = (GUIStyle)"ToolbarSeachCancelButton";
#endif
       
      AddEntryTranslationSearchString = EditorGUILayout.TextField(AddEntryTranslationSearchString, searchFieldText,GUILayout.Width(rect.width-8));
      var finalHeight =175;
       // 过滤组件
       var filteredTypes = _state.GetAllCanUseEntryTransitions()
         .Where(t =>
         {
           return t != null && t.Name.Contains(AddEntryTranslationSearchString);
         })
         .ToList();
               
       // 滚动列表
       EditorGUILayout.BeginVertical();
       AddEntryTransitionPos = EditorGUILayout.BeginScrollView(AddEntryTransitionPos,false,false,GUILayout.Width(addRect.width-8),GUILayout.Height(finalHeight));
       var style = new GUIStyle(EditorStyles.toolbarButton);
       style.alignment = TextAnchor.MiddleLeft;
       foreach (var type in filteredTypes)
       {
         if (GUILayout.Button(new GUIContent(type.Name), style,
               GUILayout.MaxWidth(addRect.width)))
         {
           _state.AddExitTransition(type,_state);
           ShowAddExitTransition = false;
         }
       }


       EditorGUILayout.EndScrollView();
       EditorGUILayout.EndVertical();
       var scrollRect = GUILayoutUtility.GetLastRect().x !=0 ? GUILayoutUtility.GetLastRect() : oldExitIgnoreScrollWhellRect;
       
       if (scrollRect.Contains(Event.current.mousePosition))
       {
         NodeEditorWindow.shouldIgnoreMouseScroll = true;
       }

       oldExitIgnoreScrollWhellRect = scrollRect;
     }
     GUILayout.EndVertical();
     GUILayout.EndHorizontal();
     GUI.color = Color.white;

      EditorGUILayout.EndVertical();

      _enterRect = GUILayoutUtility.GetLastRect();


      foreach (var t in needExitRemovePortTranslation)
      {
        try
        {
          _state.RemoveExitTransition(t);
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
          throw;
        }
      }
      needExitRemovePortTranslation.Clear();
      
      
     // EditorGUILayout.EndVertical();
      _exitRect = GUILayoutUtility.GetLastRect();
    }

    private void UpdateAllNodeWithTranlationAttribute()
    {
      UpdateFieldEntryAttribute();
      UpdateFieldExitAttribute();
    }

    private void UpdateFieldEntryAttribute()
    {
      var fsmStateNode = target as AFSMStateNode;
      var type = fsmStateNode.GetType();
      var allFieldWithTranslationAttribute = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(t =>
        t.IsDefined(typeof(EntryTranslationAttribute), true) && typeof(BaseEntryTranslation).IsAssignableFrom(t.FieldType));
      //清除原始数据

      foreach (var afsmStateTranslation in fsmStateNode.fieldEntryTranslations)
      {
        if (afsmStateTranslation == null ||afsmStateTranslation.Name == null)
        {
          continue;
        }
        if (fsmStateNode.GetPort("entry" + afsmStateTranslation.Name) != null &&
            !allFieldWithTranslationAttribute.Any((f) => f.FieldType == afsmStateTranslation.GetType() && f.Name == afsmStateTranslation.Name))
        {
          fsmStateNode.RemoveDynamicPort("entry" + afsmStateTranslation.Name);
        }
      }

      fsmStateNode.fieldEntryTranslations.Clear();

      
      foreach (var fieldInfo in allFieldWithTranslationAttribute)
      {
        var instance = (AFSMStateTranslation)fieldInfo.GetValue(fsmStateNode);
        if (instance is null)
        {
          instance = Activator.CreateInstance(fieldInfo.FieldType,fsmStateNode,fieldInfo.Name) as AFSMStateTranslation;
          instance.Name = fieldInfo.Name;
          instance.PortName = fieldInfo.Name;
          instance.IO = NodePort.IO.Input;
          instance.Node = fsmStateNode;
        }
        
        if (instance.Name == null || instance.Name == "")
        {
          instance.Name = fieldInfo.Name;
          instance.PortName = fieldInfo.Name;
          instance.IO = NodePort.IO.Input;
          instance.Node = fsmStateNode;
        }

        fsmStateNode.fieldEntryTranslations.Add(instance);
        var port = fsmStateNode.GetInputPort("entry" + instance.Name);
        //port.AddConnections();
        if (port == null)
        {
          fsmStateNode.AddDynamicInput(typeof(AFSMStateTranslation), XNode.Node.ConnectionType.Override, XNode.Node.TypeConstraint.Inherited,
            "entry" + instance.Name);
        }
      }
    }

    private void UpdateFieldExitAttribute()
    {
      //清除原始数据
      var fsmStateNode = target as AFSMStateNode;
      var type = fsmStateNode.GetType();
      var allFieldWithTranslationAttribute = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(t =>
        t.IsDefined(typeof(ExitTranslationAttribute), true) && typeof(BaseExitTranslation).IsAssignableFrom(t.FieldType));
      foreach (var afsmStateTranslation in fsmStateNode.fieldExitTranslations)
      {
        if (afsmStateTranslation == null ||afsmStateTranslation.Name == null)
        {
          continue;
        }
        if (fsmStateNode.GetPort("exit" + afsmStateTranslation.Name) != null &&
            !allFieldWithTranslationAttribute.Any((f) => f.FieldType == afsmStateTranslation.GetType() && f.Name == afsmStateTranslation.Name))
        {
          fsmStateNode.RemoveDynamicPort("exit" + afsmStateTranslation.Name);
        }
      }

      fsmStateNode.fieldExitTranslations.Clear();


      foreach (var fieldInfo in allFieldWithTranslationAttribute)
      {
        var instance = (AFSMStateTranslation)fieldInfo.GetValue(fsmStateNode);
        if (instance is null)
        {
          instance = Activator.CreateInstance(fieldInfo.FieldType,fsmStateNode,fieldInfo.Name) as AFSMStateTranslation;
          instance.Name = fieldInfo.Name;
          instance.PortName = fieldInfo.Name;
          instance.IO = NodePort.IO.Output;
          instance.Node = fsmStateNode;
          fieldInfo.SetValue(fsmStateNode, instance);
        }
        if (instance.Name == null || instance.Name == "")
        {
          instance.Name = fieldInfo.Name;
          instance.PortName = fieldInfo.Name;
          instance.IO = NodePort.IO.Output;
          instance.Node = fsmStateNode;
        }

        fsmStateNode.fieldExitTranslations.Add(instance);
        var port = fsmStateNode.GetOutputPort("exit" + instance.Name);
        if (port == null)
        {
          fsmStateNode.AddDynamicOutput(typeof(AFSMStateTranslation), XNode.Node.ConnectionType.Override, XNode.Node.TypeConstraint.Inherited,
            "exit" + instance.Name);
        }
      }
    }

        public void CleanAllNonTransaltionPorts()
        {
          var state = target as AFSMStateNode;
          HashSet<string> allTranslationPortNames = new HashSet<string>();
          HashSet<NodePort> shouldCleanPorts = new HashSet<NodePort>();
          foreach (var translation in state.fieldEntryTranslations) allTranslationPortNames.Add("entry" +translation.PortName);
          foreach (var translation in state.fieldExitTranslations) allTranslationPortNames.Add("exit" +translation.PortName);
          foreach (var translation in state.Entries) allTranslationPortNames.Add("entry" +translation.PortName);
          foreach (var translation in state.Exits) allTranslationPortNames.Add("exit" +translation.PortName);
          foreach (var port in state.Ports)
          {
            if (port.fieldName.StartsWith("entry") || port.fieldName.StartsWith("exit"))
            {
              if (!allTranslationPortNames.Contains(port.fieldName))
              {
                  shouldCleanPorts.Add(port);
              }
            }
          }

          foreach (var shouldCleanPort in shouldCleanPorts)
          {
             state.RemoveDynamicPort(shouldCleanPort);
          }
          
        }

        public override void AddContextMenuItems(GenericMenu menu)
        {
          base.AddContextMenuItems(menu);
          menu.AddItem(new GUIContent("Set As EntryState"),false, () =>
          {
            if (target is AFSMStateNode node)
            {
               node.FSM.SetEntryState(node);
            }
          });
        }

        public override Color GetTint()
    {
      _state = target as AFSMStateNode;
      if (_state.FSM.CurrentState&& _state.FSM.CurrentState == _state)
      {
          return new Color(Color.yellow.r * 0.4f,Color.yellow.g * 0.4f,Color.yellow.b * 0.4f,1);
      }

      if (_state.FSM.EntryState == _state)
      {
         return new Color(0f,0.4f,0f,1f);
      }
      return base.GetTint();
    }
  }
}