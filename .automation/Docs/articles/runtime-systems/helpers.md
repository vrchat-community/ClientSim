---
uid: helpers
---

# VRCSDK Helpers

The Helper components are added to an object to help with handling the behavior of VRC SDK components. The role of these components remains the same compared to CyanEmu and Phase 2, although some logic not specific to the function of the object itself has been stripped out. As an example, in CyanEmu the CyanEmuPickupHelper script handled the logic for holding pickups. Now this behavior has been moved outside the pickup helper class, and into the pickup management system. The PickupHelper code now only provides data for how the PlayerHand should handle the pickup.

Helper classes may also extend interfaces that are used in ClientSim. There are two categories of interfaces: [Usables](#usable-interfaces) and [Handlers](#handler-interfaces). 

## Usable Interfaces

Usable interfaces normally end in “able”, and represent items that can be used somehow within ClientSim. They provide information on how they can be used, but do not include the methods to use them.

| Name                       | Description                                                      |
|----------------------------|------------------------------------------------------------------|
| [IClientSimInteractable    ](xref:VRC.SDK3.ClientSim.IClientSimInteractable) | Represents an object that can be interacted with                 |
| [IClientSimPickupable      ](xref:VRC.SDK3.ClientSim.IClientSimPickupable) | Represents an object that can be picked up, Extends Interactable |
| [IClientSimStation         ](xref:VRC.SDK3.ClientSim.IClientSimStation) | Represents an object that the player can use to sit              |
| [IClientSimSyncable        ](xref:VRC.SDK3.ClientSim.IClientSimSyncable) | Represents an object that can have an owner                      |
| [IClientSimPositionSyncable](xref:VRC.SDK3.ClientSim.IClientSimPositionSyncable) | Represents an object that syncs its position, Extends Syncable   |

## Handler Interfaces

Using these two interface types, the Helper classes are ways of wrapping VRChat SDK component information to provide it to ClientSim.

| Name                       | Description                                                      |
|----------------------------|------------------------------------------------------------------|
| [PositionSyncedHelperBase](xref:VRC.SDK3.ClientSim.ClientSimPositionSyncedHelperBase)| Helper for VRCObjectSync,  Extends PositionSyncedHelperBase. Syncable, PositionSyncable, RespawnHandler |
| [ObjectSyncHelper](xref:VRC.SDK3.ClientSim.ClientSimObjectSyncHelper)| Helper for VRCObjectSync, Extends PositionSyncedHelperBase. Syncable, PositionSyncable, RespawnHandler |
| [UdonHelper](xref:VRC.SDK3.ClientSim.ClientSimUdonHelper)          | Helper for UdonBehaviour, Extends PositionSyncedHelperBase. Syncable, PositionSyncable, RespawnHandler, Interactable, PickupHandler, StationHandler, SyncableHandler |
| [PickupHelper](xref:VRC.SDK3.ClientSim.ClientSimPickupHelper)         | Helper for VRCPickup. Pickupable |
| [StationHelper](xref:VRC.SDK3.ClientSim.ClientSimStationHelper) | Helper for VRCStation. Implements IClientSimStation |
| [ObjectPoolHelper](xref:VRC.SDK3.ClientSim.ClientSimObjectPoolHelper) | Helper for VRCObjectPool. Syncable |
| [CombatSystemHelper](xref:VRC.SDK3.ClientSim.ClientSimCombatSystemHelper) | Helper for Udon CombatSetup. Implements IVRC_Destructible. Helper component is added to the player object directly when initialized. |
| [SpatialAudioHelper](xref:VRC.SDK3.ClientSim.ClientSimSpatialAudioHelper) | Helper for VRCSpatialAudioSource |