namespace ilsFramework.Core
{
    public class SubFSM : FSM,IState
    {
        public FSM Owner { get; set; }
        public bool IsExecuting { get; set; }
        public virtual void OnInit()
        {
            
        }

        public virtual void OnEnter()
        {
            _currentState.IsExecuting = true;
            _currentState.OnEnter();
        }

        public virtual void OnUpdate()
        {
            _currentState.OnUpdate();
        }

        public virtual void OnLateUpdate()
        {
            _currentState.OnLateUpdate();
        }

        public virtual void OnLogicUpdate()
        {
            _currentState.OnLogicUpdate();
        }

        public virtual void OnFixedUpdate()
        {
            _currentState.OnFixedUpdate();
        }

        public virtual void OnExit()
        {
            _currentState.IsExecuting = false;
            _currentState.OnExit();
        }
    }
}