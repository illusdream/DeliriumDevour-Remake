using System;
using System.Collections.Generic;
using System.Linq;
using ilsActionEditor.Test;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using XNode;

namespace ilsActionEditor
{
    [ilsActionEditor.NodeWidth(350)]
    [NodeWidth(350)]
    [NodeMenuItem("Base/AFSMStateNode")]
    public class AFSMStateNode : AFSMNode
    {
        #region DynamicTranlation

        [SerializeReference]
        [Sirenix.OdinInspector.ShowIf("ShowTranslations",true)]
        [DisableContextMenu(DisableForCollectionElements = true)]
        [HideReferenceObjectPicker]
        private List<AFSMStateTranslation> entries = new List<AFSMStateTranslation>();
        [SerializeField]
        [HideInInspector]
        private int MaxiumEntriesIndex;
        
        [SerializeReference]
        [Sirenix.OdinInspector.ShowIf("ShowTranslations",true)]
        [DisableContextMenu(DisableForCollectionElements = true)]
        [HideReferenceObjectPicker]
        private List<AFSMStateTranslation> exits = new List<AFSMStateTranslation>();
        [SerializeField]
        [HideInInspector]
        private int MaxiumExitsIndex;
#if UNITY_EDITOR
        public List<AFSMStateTranslation> Exits=> exits;
        public List<AFSMStateTranslation> Entries=> entries;
#endif

        #endregion

        #region FieldTranlation

        [SerializeReference]
        [Sirenix.OdinInspector.ShowIf("ShowTranslations",true)]
        [DisableContextMenu(DisableForCollectionElements = true)]
        [HideReferenceObjectPicker]
        public List<AFSMStateTranslation> fieldEntryTranslations = new List<AFSMStateTranslation>();
        
        [SerializeReference]
        [Sirenix.OdinInspector.ShowIf("ShowTranslations",true)]
        [DisableContextMenu(DisableForCollectionElements = true)]
        [HideReferenceObjectPicker]
        public List<AFSMStateTranslation> fieldExitTranslations = new List<AFSMStateTranslation>();
        #endregion
        
      //  [HideInInspector]
        [FormerlySerializedAs("ShowTest")] public bool ShowTranslations;

        
        public BiMap<AFSMStateTranslation,NodePort> Mapping_Port22Translation;
 
        public void Initialize()
        {
            foreach (var entry in fieldEntryTranslations) entry.Initialize(this);
            foreach (var entry in entries) entry.Initialize(this);
            foreach (var exit in fieldExitTranslations) exit.Initialize(this);
            foreach (var exit in exits) exit.Initialize(this);
            BuildMapping_Port22Translation();
            OnInit();
        }

        public void BuildMapping_Port22Translation()
        {
            Mapping_Port22Translation = new BiMap<AFSMStateTranslation,NodePort>();
            foreach (var entry in fieldEntryTranslations)
            {
                Mapping_Port22Translation.TryAdd(entry,entry.GetPort());
            }

            foreach (var exit in fieldExitTranslations)
            {
                Mapping_Port22Translation.TryAdd(exit,exit.GetPort());
            }
            foreach (var entry in entries)
            {
                Mapping_Port22Translation.TryAdd(entry,entry.GetPort());
            }

            foreach (var exit in exits)
            {
                Mapping_Port22Translation.TryAdd(exit,exit.GetPort());
            }
        }
        public virtual void OnInit(){}

        
        public void Enter()
        {
            foreach (var entry in fieldEntryTranslations) entry.StateEnter();
            foreach (var entry in entries) entry.StateEnter();
            foreach (var exit in fieldExitTranslations) exit.StateEnter();
            foreach (var exit in exits) exit.StateEnter();

            OnEnter();
        }
        public virtual void OnEnter(){}

        public void DoUpdate(float deltaTime)
        {
            OnUpdate(deltaTime);
        }

        public virtual void OnUpdate(float deltaTime){}

        public void LogicUpdate()
        {
            OnLogicUpdate();
        }
        public virtual void OnLogicUpdate() {}

        public void FixedUpdate()
        {
            OnFixedUpdate();
        }
        public virtual void OnFixedUpdate() {}

        public void LateUpdate()
        {
            OnLateUpdate();
        }
        public virtual void OnLateUpdate() {}

        public void Exit()
        {
            foreach (var entry in fieldEntryTranslations) entry.StateExit();
            foreach (var entry in entries) entry.StateExit();
            foreach (var exit in fieldExitTranslations) exit.StateExit();
            foreach (var exit in exits) exit.StateExit();
            OnExit();
        }
        public virtual void OnExit(){}

        public void Destroy()
        {
            OnStateDestroy();
        }
        public virtual void OnStateDestroy(){}


        public AFSMStateNode GetTransition(float deltaTime)
        {
            foreach (var entry in fieldEntryTranslations) entry.Update(deltaTime);
            foreach (var entry in entries) entry.Update(deltaTime);
            foreach (var exit in fieldExitTranslations) exit.Update(deltaTime);
            foreach (var exit in exits) exit.Update(deltaTime);
            
            AFSMStateNode output = null;
            foreach (var exitTranslation in fieldExitTranslations)
            {
                if (exitTranslation == null)
                {
                    continue;
                }
                if (exitTranslation.GetConnection() != null&&  exitTranslation.CanTranslate())
                {
                    if (exitTranslation.GetNextStateNodeEntryTranslation<AFSMStateTranslation>().CanTranslate())
                    {
                        output = exitTranslation.TargetNextState();
                    }

                }
                if (output)
                {
                    return output;
                }
            }
            foreach (var exitTranslation in exits)
            {
                if (exitTranslation == null)
                {
                    continue;
                }
                if (exitTranslation.GetConnection() != null &&exitTranslation.CanTranslate())
                {
                    if (exitTranslation.GetNextStateNodeEntryTranslation<AFSMStateTranslation>().CanTranslate())
                    {
                        output = exitTranslation.TargetNextState();
                    }
                }

                if (output)
                {
                    return output;
                }
            }
            return null;
            
        }

        public AFSMStateTranslation GetTranslation(NodePort port)
        {
            return GetValue(port) as AFSMStateTranslation;
        }
        
        public override object GetValue(NodePort port)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                foreach (var t in entries.Where(t => t.GetPort() == port)) return t;
                foreach (var t in exits.Where(t => t.GetPort() == port)) return t;
                foreach (var t in fieldEntryTranslations.Where(t => t.GetPort() == port)) return t;
                foreach (var t in fieldExitTranslations.Where(t => t.GetPort() == port))return t;
                return null;
            }
#endif
            if (Mapping_Port22Translation == null)
            {
                return GetValueWithoutTranslation(port);
            }
            if (Mapping_Port22Translation.TryGetLeft(port, out AFSMStateTranslation translation))
            {
                return translation;
            }
            //其他乱七八糟的
            return GetValueWithoutTranslation(port);
        }

        public virtual object GetValueWithoutTranslation(NodePort port)
        {
            return base.GetValue(port);
        }


        public int EntriesCount => entries.Count;
        public AFSMStateTranslation GetEntryConnection(int index)
        {
            return entries.Count > index ? entries[index] : null;
        }
        public AFSMStateTranslation AddEntryTransition(Type type,XNode.Node node)
        {
            if (!typeof(AFSMStateTranslation).IsAssignableFrom(type))
            {
                return null;
            }
            var entry = Activator.CreateInstance(type,node,(MaxiumEntriesIndex).ToString()) as AFSMStateTranslation;
            AddDynamicInput(typeof(AFSMStateTranslation), XNode.Node.ConnectionType.Override, XNode.Node.TypeConstraint.Inherited,
                "entry" + (MaxiumEntriesIndex));
            entry.IO = NodePort.IO.Input;
            entries.Add(entry);
            MaxiumEntriesIndex++;
            return entry;

        }

        public void RemoveEntryTransition(string targetPort)
        {
            var result=  entries.Find((t)=>t.PortName == targetPort);
            result.LogSelf();
            if (GetPort("entry" +result.PortName) !=null)
            {
                RemoveDynamicPort("entry" +result.PortName);
            }
            entries.Remove(result);
        }
        
        public int ExitsCount => exits.Count;

        public AFSMStateTranslation GetExitConnection(int index)
        {
            return exits.Count > index ? exits[index] : null;
        }
        
        public AFSMStateTranslation AddExitTransition(Type type,XNode.Node node)
        {
            if (!typeof(AFSMStateTranslation).IsAssignableFrom(type))
            {
                return null;
            }
            var exit = Activator.CreateInstance(type,node,(MaxiumExitsIndex).ToString()) as AFSMStateTranslation;
            AddDynamicOutput(typeof(AFSMStateTranslation), XNode.Node.ConnectionType.Override, XNode.Node.TypeConstraint.Inherited,
                "exit" + (MaxiumExitsIndex));
            exit.IO = NodePort.IO.Output;
            exits.Add(exit);
            MaxiumExitsIndex++;
            return exit;
        }
        
        public void RemoveExitTransition(string targetPort)
        {
            var result=  exits.Find((t)=>t.PortName == targetPort);
            if (GetPort("exit" +result.PortName) !=null)
            {
                RemoveDynamicPort("exit" +result.PortName);
            }
            exits.Remove(result);
        }
#if UNITY_EDITOR
        public virtual List<Type> GetAllCanUseEntryTransitions()
        {
            var result = UnityEditor.TypeCache.GetTypesDerivedFrom<BaseEntryTranslation>().ToArray().ToList();
            result.Add(typeof(BaseEntryTranslation));
            return result;
        }
        public virtual List<Type> GetAllCanUseExitTransitions()
        {
            var result = UnityEditor.TypeCache.GetTypesDerivedFrom<BaseExitTranslation>().ToArray().ToList();
            result.Add(typeof(BaseExitTranslation));
            return result;
        }
#endif

        
    }
}