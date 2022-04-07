# Script Execution Order

| Execution Order | System Name          | Description                                                                                                                                                    |
|-----------------|----------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| -Infinity       | UnityInputSystem     | Unity InputSystem updates before all MonoBehaviours. Input from user buttons are sent to ClientSimInput and events are dispatched.                             |
| -3000           | TrackingProvider     | Input is checked to update the TrackignProvider. For example: Desktop head X rotation.                                                                         |
| -3000           | PlayerController     | Update Player position before raycasting.                                                                                                                      |
| -2000           | PlayerRaycaster      | Update the position of the PlayerHands to TrackingProvider hand data. Raycast to find interactables in the world. This must happen before EventSystems update. |
| -1000           | Unity Event System   | Send mouse events to interact with UI. Order cannot be changed.                                                                                                |
| 0               | ClientSimBehaviours  |                                                                                                                                                                |
| 0               | UdonBehaviour        | Send Update Events to Udon Programs.                                                                                                                           |
| 1               | UdonInput            | This must happen after UdonBehaviour.Update to ensure proper event order.                                                                                      |
| 10000           | ClientSimBaseInput   | Update current frame tick for Input Events. Only needed to ensure tests and playmode act the same relating to when Input is processed.                         |
| 30000           | PlayerStationManager | Update the position of players on a station as late as possible so all other scripts have had time to evaluate first.                                          |
| 30001           | TooltipManager       | Update the position of Tooltip visuals after finalizing the player's position.                                                                                 |
| 31000           | PostLateUpdater      | VRChat's PostLateUpdate event sent to UdonBehaviours.                                                                                                          |