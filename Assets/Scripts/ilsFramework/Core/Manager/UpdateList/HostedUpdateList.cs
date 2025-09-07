using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace ilsFramework.Core
{
    public class HostedUpdateList<T>
    {
        private readonly List<T> _hostedUpdateList;

        private readonly List<T> _frameAddList;

        private readonly List<T> _frameRemoveList;
        
        private readonly Action<T> detailUpdateCallback;

        public HostedUpdateList(Action<T> detailUpdateCallback)
        {
            this.detailUpdateCallback = detailUpdateCallback;
            _hostedUpdateList = new List<T>();
            _frameAddList = new List<T>();
            _frameRemoveList = new List<T>();
        }
        
        public void Update()
        {
            UpdateAllChangeForHostedUpdateList();

            if (detailUpdateCallback is null)
            {
                return;
            }
            
            foreach (var hostedUpdate in _hostedUpdateList)
            {
                detailUpdateCallback.Invoke(hostedUpdate);
            }
        }

        private void UpdateAllChangeForHostedUpdateList()
        {
            foreach (var add in _frameAddList)
            {
                _hostedUpdateList.Add(add);
            }

            _frameAddList.Clear();

            foreach (var remove in _frameRemoveList)
            {
                _hostedUpdateList.Remove(remove);
            }
            
            _frameRemoveList.Clear();
        }

        public void AddUpdate(T update)
        {
            _frameAddList.Add(update);
        }

        public void RemoveUpdate(T update)
        {
            _frameRemoveList.Add(update);
        }
    }
}