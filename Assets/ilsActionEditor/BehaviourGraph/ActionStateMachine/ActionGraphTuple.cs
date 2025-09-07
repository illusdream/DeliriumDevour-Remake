using System;

namespace ilsActionEditor
{
    [Serializable]
    public class ActionGraphTuple
    {
        public string ActionGraphKey;

        public AFSMGraph ActionGraph;

        public AFSMGraph CurrentActionGraph;

        //运行时数据，直接把图的更新放这里了
        private AFSMTranslationMode _AFSMTranslationMode;

        public AFSMTranslationMode AFSMTranslationMode
        {
            get => _AFSMTranslationMode;
            set => _AFSMTranslationMode = value;
        }

        public void Initialize(AFSMTranslationMode aFSMTranslationMode, BlackBoard blackBoard)
        {
            this.AFSMTranslationMode = aFSMTranslationMode;
            //先执行Copy
            CurrentActionGraph = ActionGraph.Copy() as AFSMGraph;
            if (CurrentActionGraph)
            {
                CurrentActionGraph.Initialize(blackBoard);
                CurrentActionGraph.StartFSM();
                CurrentActionGraph.AFSMTranslationMode = AFSMTranslationMode;
            }
        }

        public void DoUpdate(float deltaTime)
        {
            CurrentActionGraph?.DoUpdate(deltaTime);
        }

        public void DoLateUpdate()
        {
            CurrentActionGraph?.LateUpdate();
        }

        public void DoFixedUpdate()
        {
            CurrentActionGraph?.FixedUpdate();
        }

        public void DoLogicUpdate()
        {
            CurrentActionGraph?.LogicUpdate();
        }

        public void OnDestroy()
        {
            CurrentActionGraph?.OnDestroy();
        }
    }
}