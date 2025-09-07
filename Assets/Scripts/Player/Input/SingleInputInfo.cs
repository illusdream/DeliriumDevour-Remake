public struct SingleInputInfo
{
        public EPlayerInput PlayerInput;
        
        public EInputInteraction InputInteraction;

        /// <summary>
        /// 第几帧的
        /// </summary>
        public int FrameID;

        /// <summary>
        /// 在同一帧中输入的Input的按时间排序的序号
        /// </summary>
        public int InputQueueIndex;
}