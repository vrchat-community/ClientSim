using System.Collections.Generic;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim.Tests
{
    public class ClientSimTestUdonInputEventListener : IClientSimUdonInputEventSender
    {
        private readonly Queue<(string, UdonInputEventArgs)> _eventQueue = new Queue<(string, UdonInputEventArgs)>();
        
        public int EventQueueCount()
        {
            return _eventQueue.Count;
        }
        
        public (string, UdonInputEventArgs) DequeueEvent()
        {
            return _eventQueue.Dequeue();
        }

        public void RunInputAction(string eventName, UdonInputEventArgs args)
        {
            _eventQueue.Enqueue((eventName, args));
        }
    }
}