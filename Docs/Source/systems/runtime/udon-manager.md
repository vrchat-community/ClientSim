# UdonManager

The UdonManager keeps track of all initialized UdonBehaviours in the scene. Note that with the VRCSDK, an UdonBehaviour will not initialize if it does not have a program. This means that legacy position-synced UdonBehaviours without programs are not tracked, even with the SyncedObjectManager. The UdonManager has two main roles. The first is to notify all Udon Helpers when ClientSim has finished initializing, which allows UdonBehaviours to start. The second is to listen for certain ClientSim [Events](event-dispatcher.md) to forward to all UdonBehaviours. Currently the UdonManager only forwards the following events:
* OnPlayerJoined
* OnPlayerLeft
* OnPlayerRespawn
