# Input

In ClientSim, all input calls are in one class to handle input and send events. The ClientSimInputManager uses the new Input System, allowing for event-driven input. It uses the PlayerInput component to gain access to the specific input events based on the Input Bindings displayed below. Since the new Unity Input System package is not included by default, and Unity requires a special setting to enable, all references to the Input System are wrapped in define conditions, which prevents errors when importing into new projects.

## Input Events

Similar to the [EventDispatcher](event-dispatcher.md), the InputManager also has its own Events that different systems can listen to directly. These events are separated from the EventDispatcher itself because all input events have similar parameters and also has input values that are not broadcasted through events but require the listening system to poll for updated axis values.

## Input Bindings

The Input System also allows for different bindings for various control schemes. See below for the included bindings: KeyboardMouse, Gamepad, and Experimental XR Controller bindings. Note that XR input bindings within the InputSystem are very limited in Unity 2019. The InputManager will need to be expanded to properly support various VR Controllers


## UdonInput

The UdonInput system is part of the InputManager Prefab, which subscribes to the proper events in the InputManager and also polls for updates on movement and look-based inputs. Due to the timing of when Unity sends input events, and when Udon should receive input events, all button-based input is queued and processed later in the frame at the same time as movement and look-based input. This queuing and processing allows input events to happen after Udon’s update method is called, similar to how it is in VRChat.