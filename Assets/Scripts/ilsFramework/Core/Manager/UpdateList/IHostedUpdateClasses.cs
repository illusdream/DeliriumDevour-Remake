namespace ilsFramework.Core
{
    public interface IBaseHostedUpdate
    {
        
    }

    public interface IHostedUpdate : IBaseHostedUpdate
    {
        public void HostedUpdate();
    }

    public interface IHostedFixedUpdate : IBaseHostedUpdate
    {
        public void HostedFixedUpdate();
    }

    public interface IHostedLateUpdate : IBaseHostedUpdate
    {
        public void HostedLateUpdate();
    }

    public interface IHostedLogicUpdate : IBaseHostedUpdate
    {
        public void HostedLogicUpdate();
    }
}