using System;
using System.Collections.Generic;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// The Event Dispatcher is how ClientSim implements the Observer Pattern.
    /// Systems can subscribe to specific event types, and other systems can then send the event.
    /// </summary>
    public class ClientSimEventDispatcher : IClientSimEventDispatcher, IDisposable
    {
        private readonly Dictionary<Type, Delegate> _eventSubscribers;

        public ClientSimEventDispatcher()
        {
            _eventSubscribers = new Dictionary<Type, Delegate>();
        }

        public void Subscribe<T>(Action<T> eventHandler) where T : IClientSimEvent
        {
            Type t = typeof(T);
            if (_eventSubscribers.TryGetValue(t, out Delegate eventDelegate))
            {
                _eventSubscribers[t] = Delegate.Combine(eventDelegate, eventHandler);
            }
            else
            {
                _eventSubscribers.Add(t, eventHandler);
            }
        }

        public void Unsubscribe<T>(Action<T> eventHandler) where T : IClientSimEvent
        {
            Type t = typeof(T);
            if (_eventSubscribers.TryGetValue(t, out Delegate eventDelegate))
            {
                Delegate remainingDelegate = Delegate.Remove(eventDelegate, eventHandler);

                if (remainingDelegate == null)
                {
                    _eventSubscribers.Remove(t);
                }
                else
                {
                    _eventSubscribers[t] = remainingDelegate;
                }
            }
        }

        /// <summary>
        /// Sends the event to subscribed receivers
        /// </summary>
        /// <param name="clientSimEvent"></param>
        /// <typeparam name="T"></typeparam>
        public void SendEvent<T>(T clientSimEvent) where T : IClientSimEvent
        {
            // TODO log warning if trying to send events while another is still being processed.

            if (_eventSubscribers.TryGetValue(typeof(T), out Delegate eventDelegate) 
                && eventDelegate is Action<T> action)
            {
                action.Invoke(clientSimEvent);
            }
        }

        public void Dispose()
        {
            _eventSubscribers.Clear();
        }
    }
}