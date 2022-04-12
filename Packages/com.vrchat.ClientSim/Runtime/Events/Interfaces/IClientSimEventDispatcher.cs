using System;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimEventDispatcher
    {
        void Subscribe<T>(Action<T> eventHandler) where T : IClientSimEvent;
        void Unsubscribe<T>(Action<T> eventHandler) where T : IClientSimEvent;
        void SendEvent<T>(T clientSimEvent) where T : IClientSimEvent;
    }
}