# Unity Event System

ClientSim uses two classes to translate actions into Unity’s EventSystem. These classes decouple Unity’s old input system into values based on ClientSim’s current bindings and match VRChat’s interactive UI object filtering. 

## BaseInput

The ClientSimBaseInput system extends Unity’s BaseInput class. Unity’s BaseInput is responsible for passing mouse position and button input into the EventSystem. The ClientSim BaseInput system overrides these methods to instead pass values based on the current ClientSim input bindings and last [PlayerRaycaster](player.md#raycaster) results. Mouse input is replaced with the current binding’s [Use Input](input.md). Since Use input is a handed action, only the value of the last activated hand is passed as mouse input. The mouse position sent to the Event System ignores the actual mouse position, and instead calculates the screen position of the last interact raycast. Using the raycast position abstracts out the real mouse’s position, allowing Desktop and VR to use Unity UI through the same system.
The BaseInput system is also responsible for providing the current mouse position to the rest of ClientSim. It controls if the mouse pointer is hidden and locked to the center of the screen, or visible and free to move. This mouse position is used for displaying the [Desktop Reticle](player.md#reticle) as well as using the mouse to create the ray direction for [DesktopRayProvider](player.md#rayprovider).

## InputModule

The ClientSimInputModule extends Unity’s StandaloneInputModule. This system processes Unity mouse events and filters out any UI objects that are not currently interactable. UI objects are interactable when all of the following conditions have been met:

1. The [PlayerRaycaster](player.md#playerraycaster) last hit an object with a VRC_UIShape component. This data is provided through ClientSimBaseInput.
2. The UI object has a UIShape component in its parent
3. The layer of the parent UIShape object is on a currently interactive layer. Interactive layers are determined by the [InteractiveLayerProvider](interactive-layer-provider.md).
4. The hit point of the UI Object raycast is contained within the collider of the UIShape. If any of those conditions fail, then the UI cannot be interacted with.