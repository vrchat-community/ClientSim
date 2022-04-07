# Automated Testing

ClientSim has many different tests to verify the behaviour of the program. The majority of the tests are Integration tests, but support for Unit tests is available. See Unity’s TestRunner to view all tests. When importing ClientSim as a package, tests can be enabled by adding the following line to the project’s package manifest after the `“dependencies” :{}` section:
```json
"testables": [
  "com.unity.inputsystem",
  "com.vrchat.clientsim"
]
```

Once added, Unity will import the tests and you will see them populated in the Test Runner Window.

![Test Runner](/images/test-runner.png)

## Unit Tests

ClientSim has a few Unit Tests that can verify items outside of Unity Playmode. More items can be refactored to split away from MonoBehaviours to be more Unit Testable.

## Integration Tests

ClientSim now has a full integration test framework that tests the majority of the features included. This framework allows for sending input events and listening for ClientSim events to verify if the proper action happened. This framework can also be used for worlds to verify specific behaviours, allowing users to create their own tests.

### Test Setup

Due to the nature of how ClientSim starts using the InitializeOnLoad, testing requires modifying Unity editor settings to properly validate behaviour. In the test environment, InitializeOnLoad happens before playmode starts. The default Unity setting has Domain Reloading enabled on entering playmode. This means that on switching to playmode, all variable data is cleared. In order to get around this, all ClientSim tests must run with Domain Reloading disabled. This is handled automatically for any test written that derives from either of the two test fixture base classes: ClientSimTestBase and ClientSimWorldTestBase. 

### Test Helpers

Both Integration Test Fixtures come with helper methods in verifying specific behaviour.

* **ClientSimTestHelpers** - This class contains helper methods to perform useful actions as well as listens to different ClientSim Events to verify actions have occurred.

* **ClientSimTestInput** - This class allows the user to set the value of any Desktop based Input event.

### ClientSimTestBase

Tests fixtures that derive from this class are for testing individual prefabs and not for testing entire worlds. On test begin, ClientSim’s default behaviour is disabled. It is possible to load a world or spawn a prefab, but ClientSim must be started manually. Depending on the order, behaviour will be different compared to starting ClientSim normally through playmode. 

1. If a world or prefab is loaded before starting ClientSim, then any VRC SDK component will not link into ClientSim and start as if ClientSim is disabled. Player spawn points will work as expected in this case as the VRC_SceneDescriptor is needed to start ClientSim and spawn a player.
2. If a world or prefab is loaded after starting ClientSim, then all VRC SDK components will initialize with ClientSim behaviours as in normal playmode. In this case though, the player will have already spawned and will not be placed at the loaded world’s spawn point. 

The majority of ClientSim tests are written in this format. A scene with the minimum components needed to start ClientSim is loaded, ClientSim is started, and then from there the tests perform what is needed, such as calling the appropriate SDK API or spawning prefabs while simulating input events. 

Here is the list of integration tests:

#### Initialization Tests
* Test the behaviour of ClientSim startup given different settings and initial scene objects.

#### Helper Tests
* Test the behaviour of various ClientSim SDK helper classes. AudioSpatializer, AVProVideoPlayer, ObjectPool, ObjectSync, Udon component without program, UIShape.

#### Interact Tests
* Test the interact system for handling interactable objects. Note that since Udon cannot be properly included in packages due to needing external references and are compiled often, this test uses a mock interactable object script

#### Pickup Tests
* Test the interaction system, player hand, and input on different pickup situations.

#### Player Api Tests
* Test behaviour for all exposed methods relating to VRCPlayerApi

#### Player Controller Tests
* Test Player locomotion settings.

#### Station Tests
* Tests using stations and expected behaviour with them.

#### UI Tests
* Test interactions with Unity UI using the VRC_UIShape component.

### ClientSimWorldTestBase

Test fixtures that derive from this class are for testing full worlds and verifying the startup of ClientSim for the given world. The test is required to load a given world in the setup phase of the test, and then ClientSim will start normally as it would outside of the test environment by entering playmode. Due to ClientSim being started normally, only one test may be run at a time as playmode is only started once for all tests. If multiple tests are run together, they will all immediately fail with a warning mentioning that only one test can run at a time.

Three World tests are provided by default:
#### No world descriptor
* Test that ClientSim will fail to start if a scene is loaded without a world descriptor

#### Two Players
* Start ClientSim normally in a basic world, spawn a remote player and verify all data on both players.

#### WorldTestExample
* This is an example test showing what it would be like for a user to write tests for their world. Test is included as a Sample for the ClientSim package and must be imported. Test shows how one would verify a simple “Puzzle” world.
