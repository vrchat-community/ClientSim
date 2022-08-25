# VRCSDK Helpers

The Helper components are added to an object to help with handling the behavior of VRC SDK components. The role of these components remains the same compared to CyanEmu and Phase 2, although some logic not specific to the function of the object itself has been stripped out. As an example, in CyanEmu the CyanEmuPickupHelper script handled the logic for holding pickups. Now this behavior has been moved outside the pickup helper class, and into the pickup management system. The PickupHelper code now only provides data for how the PlayerHand should handle the pickup.

Helper classes may also extend interfaces that are used in ClientSim. There are two categories of interfaces: [Usables](#usable-interfaces) and [Handlers](#handler-interfaces). 

## Usable Interfaces

Usable interfaces normally end in “able”, and represent items that can be used somehow within ClientSim. They provide information on how they can be used, but do not include the methods to use them.

| Name                       | Description                                                      |
|----------------------------|------------------------------------------------------------------|
| IClientSimInteractable     | Represents an object that can be interacted with                 |
| IClientSimPickupable       | Represents an object that can be picked up, Extends Interactable |
| IClientSimStation          | Represents an object that the player can use to sit              |
| IClientSimSyncable         | Represents an object that can have an owner                      |
| IClientSimPositionSyncable | Represents an object that syncs its position, Extends Syncable   |

## Handler Interfaces

Using these two interface types, the Helper classes are ways of wrapping VRChat SDK component information to provide it to ClientSim.

| Name                       | Description                                                      |
|----------------------------|------------------------------------------------------------------|
| PositionSyncedHelperBase   | Helper for VRCObjectSync,  Extends PositionSyncedHelperBase. Syncable, PositionSyncable, RespawnHandler |
| ObjectSyncHelper| Helper for VRCObjectSync, Extends PositionSyncedHelperBase. Syncable, PositionSyncable, RespawnHandler |
| UdonHelper          | Helper for UdonBehaviour, Extends PositionSyncedHelperBase. Syncable, PositionSyncable, RespawnHandler, Interactable, PickupHandler, StationHandler, SyncableHandler |
| PickupHelper     | Helper for VRCPickup. Pickupable |
| StationHelper | Helper for VRCStation. Implements IClientSimStation |
| ObjectPoolHelper| Helper for VRCObjectPool. Syncable |
| CombatSystemHelper | Helper for Udon CombatSetup. Implements IVRC_Destructible. Helper component is added to the player object directly when initialized. |
| SpatialAudioHelper | Helper for VRCSpatialAudioSource |