using NUnit.Framework;

namespace VRC.SDK3.ClientSim.Editor.Tests
{
    public class ClientSimEventDispatcherUnitTests
    {
        private class ClientSimTestEvent1 : IClientSimEvent { }
        private class ClientSimTestEvent2 : IClientSimEvent { }
        
        private class ClientSimTestListener
        {
            public bool receivedEvent1;
            public bool receivedEvent2;
            
            public void OnEvent(ClientSimTestEvent1 event1)
            {
                receivedEvent1 = true;
            }
            
            public void OnEvent(ClientSimTestEvent2 event2)
            {
                receivedEvent2 = true;
            }
        }
        
        [Test]
        public void TestSingleListenerSendEvent()
        {
            ClientSimEventDispatcher dispatcher = new ClientSimEventDispatcher();
            ClientSimTestListener listener = new ClientSimTestListener();
            ClientSimTestEvent1 event1 = new ClientSimTestEvent1();
            
            // Verify initial value is false
            Assert.IsFalse(listener.receivedEvent1);
            
            // Send the event before subscribing and verify value was not updated.
            dispatcher.SendEvent(event1);
            Assert.IsFalse(listener.receivedEvent1);
            
            // Subscribe to the event, send the event, and verify it was called.
            dispatcher.Subscribe<ClientSimTestEvent1>(listener.OnEvent);
            dispatcher.SendEvent(event1);
            Assert.IsTrue(listener.receivedEvent1);
            
            // Test unsubscribing.
            listener.receivedEvent1 = false;
            dispatcher.Unsubscribe<ClientSimTestEvent1>(listener.OnEvent);
            dispatcher.SendEvent(event1);
            Assert.IsFalse(listener.receivedEvent1);
        }
        
        [Test]
        public void TestMultiListenerSendEvent()
        {
            ClientSimEventDispatcher dispatcher = new ClientSimEventDispatcher();
            ClientSimTestListener listener1 = new ClientSimTestListener();
            ClientSimTestListener listener2 = new ClientSimTestListener();
            ClientSimTestEvent1 event1 = new ClientSimTestEvent1();
            
            // Send the event before subscribing and verify value was not updated.
            dispatcher.SendEvent(event1);
            Assert.IsFalse(listener1.receivedEvent1);
            Assert.IsFalse(listener2.receivedEvent1);
            
            // Subscribe to the event, send the event, and verify it was called.
            dispatcher.Subscribe<ClientSimTestEvent1>(listener1.OnEvent);
            dispatcher.Subscribe<ClientSimTestEvent1>(listener2.OnEvent);
            dispatcher.SendEvent(event1);
            Assert.IsTrue(listener1.receivedEvent1);
            Assert.IsTrue(listener2.receivedEvent1);
            
            // Test unsubscribing for one event
            listener1.receivedEvent1 = false;
            listener2.receivedEvent1 = false;
            dispatcher.Unsubscribe<ClientSimTestEvent1>(listener1.OnEvent);
            dispatcher.SendEvent(event1);
            Assert.IsFalse(listener1.receivedEvent1);
            Assert.IsTrue(listener2.receivedEvent1);
        }

        [Test]
        public void TestMultiEventSendEvent()
        {
            ClientSimEventDispatcher dispatcher = new ClientSimEventDispatcher();
            ClientSimTestListener listener = new ClientSimTestListener();
            ClientSimTestEvent1 event1 = new ClientSimTestEvent1();
            ClientSimTestEvent2 event2 = new ClientSimTestEvent2();
            
            // Send the event before subscribing and verify value was not updated.
            dispatcher.SendEvent(event1);
            Assert.IsFalse(listener.receivedEvent1);
            Assert.IsFalse(listener.receivedEvent2);
            
            // Subscribe to the event, send the event, and verify it was called.
            dispatcher.Subscribe<ClientSimTestEvent1>(listener.OnEvent);
            dispatcher.Subscribe<ClientSimTestEvent2>(listener.OnEvent);
            dispatcher.SendEvent(event1);
            Assert.IsTrue(listener.receivedEvent1);
            Assert.IsFalse(listener.receivedEvent2);
            
            listener.receivedEvent1 = false;
            
            // Test sending second event.
            dispatcher.SendEvent(event2);
            Assert.IsFalse(listener.receivedEvent1);
            Assert.IsTrue(listener.receivedEvent2);
            
            listener.receivedEvent2 = false;
            
            // Test unsubscribing events
            dispatcher.Unsubscribe<ClientSimTestEvent1>(listener.OnEvent);
            dispatcher.Unsubscribe<ClientSimTestEvent2>(listener.OnEvent);
            dispatcher.SendEvent(event1);
            dispatcher.SendEvent(event2);
            Assert.IsFalse(listener.receivedEvent1);
            Assert.IsFalse(listener.receivedEvent2);
        }
    }
}
