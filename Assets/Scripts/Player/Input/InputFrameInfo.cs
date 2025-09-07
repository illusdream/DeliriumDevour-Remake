using System.Collections.Generic;
using Sirenix.OdinInspector;

public class InputFrameInfo
{
        [ShowInInspector]
        private List<SingleInputInfo> InputInfos = new List<SingleInputInfo>();

        private Dictionary<(EPlayerInput, EInputInteraction), SingleInputInfo> DetailInputInfos = new Dictionary<(EPlayerInput, EInputInteraction), SingleInputInfo>();

        public void AddInputInfo(SingleInputInfo inputInfo)
        {
                inputInfo.InputQueueIndex = InputInfos.Count;
                InputInfos.Add(inputInfo);
                DetailInputInfos[(inputInfo.PlayerInput, inputInfo.InputInteraction)] = inputInfo;
        }

        public bool TryGetInputInfo((EPlayerInput, EInputInteraction) inputType, out SingleInputInfo inputInfo)
        {
                return DetailInputInfos.TryGetValue(inputType, out inputInfo);
        }
        
}