using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ilsFramework.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ilsActionEditor.Editor
{
    public class BasePortView : VisualElement
    {
        public BasePortPointView view { get; private set; }
        
        public VisualElement PortPointContainer { get; private set; }
        
        public VisualElement PortContentContainer { get; private set; }

        public BaseNodeView NodeView { get; private set; }

        public bool DontModifier;

        public AFSMStateTranslation Translation { get; private set; }
        
        public Dictionary<FieldInfo,VisualElement> allFields = new Dictionary<FieldInfo,VisualElement>();
        public BasePortView(AFSMStateTranslation translation,BasePortPointView portPointView,BaseNodeView nodeView)
        {
            Translation = translation;
            view = portPointView;
            NodeView = nodeView;
            contentContainer.style.flexDirection = FlexDirection.Column;
            PortPointContainer = new VisualElement();
            PortPointContainer.Add(view);
            PortContentContainer = new VisualElement();
            Insert(0,PortPointContainer);
            Insert(1,PortContentContainer);
            this.AddManipulator((IManipulator) new ContextualMenuManipulator(new Action<ContextualMenuPopulateEvent>(this.BuildContextualMenu)));
            
            
            RegisterCallback<MouseDownEvent>(OnClickPortView);
            GenerateAllPortFields();
        }

        public virtual void GenerateAllPortFields()
        {
            DefaultGenerateAllPortFields(); 
        }

        public void DefaultGenerateAllPortFields()
        {
            var fields = Translation.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            //只关注在AFSMNode后生成的
                .Where(f => f.DeclaringType != typeof(AFSMNode) 
                            && (f.CustomAttributes.Count(attr => attr.AttributeType == typeof(ShowInGraphAttribute)) >0));
            foreach (var field in fields)
            {
                var view = CreateFieldsView(field,ObjectNames.NicifyVariableName(field.Name));
            }
        }

        public VisualElement CreateFieldsView(FieldInfo field, string label = null, Action valueChangedCallback = null)
        {
            if(field == null)
                return null;

            var view = FieldFactory.CreateField(field.FieldType, field.GetValue(Translation), (newValue) =>
            {
                field.SetValue(Translation, newValue);
                valueChangedCallback?.Invoke();
            }, label);

            allFields.Add(field,view);
            PortContentContainer.Add(view);
            
            return view;
        }
        
        public virtual void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Remove", new Action<DropdownMenuAction>(RemoveSelf), new Func<DropdownMenuAction, DropdownMenuAction.Status>(CanModifier));
            evt.menu.AppendAction("Rename", new Action<DropdownMenuAction>(Rename), new Func<DropdownMenuAction, DropdownMenuAction.Status>(CanModifier));
            evt.menu.AppendSeparator();
        }

        public void RemoveSelf(DropdownMenuAction action)
        {
            if (DontModifier)
            {
                return;
            }
            NodeView.RemovePort(this);
        }

        public DropdownMenuAction.Status CanModifier(DropdownMenuAction action)
        {
            return DontModifier ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal;
        }

        public void Rename(DropdownMenuAction action)
        {
            if (DontModifier)
            {
                return;
            }
            NodeView.RenamePort(this);
        }
        
        
        private void OnClickPortView(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                var portContainer = ScriptableObject.CreateInstance<PortContainer>();
                portContainer.transition = Translation;
                portContainer.name = $"{Translation.GetType().Name} : {Translation.Name}";
                portContainer.PortInspectorValueChange += PortContainerOnPortInspectorValueChange;
                Selection.activeObject = portContainer;
                evt.StopPropagation();
            }
        }

        private void PortContainerOnPortInspectorValueChange()
        {
            foreach (var field in allFields)
            {
                var result = field.Value.GetType().GetMethod("SetValueWithoutNotify");
                if (result != null)
                {
                    result.Invoke(field.Value, new []{field.Key.GetValue(Translation)});
                }
            }
        }
    }
}