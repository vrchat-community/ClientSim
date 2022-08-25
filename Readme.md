# ClientSim

![ClientSim in Unity Editor](Tools/Docusaurus/static/images/editor-screenshot.png)

The VRChat Client Simulator, or ClientSim for short, is a tool that enables you to test your VRChat SDK3 Worlds directly in Unity! You can look at the state of all objects to verify things directly.

## Features

- Debug everything in Unity.
- Inspect Udon variables in Play Mode.
- Desktop player controller.
- Grab Pickups, use Interacts, UI and Stations.
- Delete EditorOnly objects on Play.

## Setup

### Requirements

- Unity 2019.4.31
- VRChat Creator Companion

### Installing

Download and install the [Creator Companion](https://vrchat.com/home/download) if you do not have it yet.
Then you can create a new project from either the "World" or "UdonSharp" Templates, and ClientSim will be included and ready to go.

If you want to add ClientSim to an existing project, you can [migrate it](https://vcc.docs.vrchat.com/vpm/migrating#the-process) using the VCC, then find ClientSim in the Package Listing and press 'Add'. If you migrate a project which has CyanEmu in it, that will automatically be swapped out for the latest ClientSim version as part of Migration.

### Getting started

- Open your VRChat world scene
- Press play in Unity
- Test your world


## Learn more about how all the systems work in the [Systems](https://vrchat-community.github.io/ClientSim/systems/index.html) section

### New Features in ClientSim compared to CyanEmu
- Pickup manipulation through I, J, K, L, U, O, mouse scrolling and gamepad.
- Input based on keyboard layout isntead of specific keys.
- Local and Remote player now have humanoid avatars, avatar bones supported (but not full Avatar systems).
- View and set player data directly - Locomotion values, voice and avatar audio settings, combat health.
- Unlock mouse by holding tab - you can highlight off-center objects this way.
- Pointer displays when a UI element can be interacted with.
- New runtime options: start as non-master, invert mouse, show tooltips, change player scale, set target framerate, delay start to simulate loading.
- New playmode menu with updated style, player info and settings buttons.
- New buttons in the Settings Window to update Project Settings, no longer happens without user consent.
- Mesh highlighting that matches Android.
- Tooltip location updated to match client.
- Better gamepad support.
- Support for Disabled Domain Reload to enter playmode without delay - note that there is a Unity bug that causes UI Events to fail if they're not set to "Editor and Runtime".
- Automated Testing - you need to specifically enable this so it won't affect your project. Documentation forthcoming.

### Known Issues

- Manually changing Unity Project settings to enable the new input system may not properly allow input. Users should use the buttons in the ClientSim Settings Window.
- Physics.RaycastNonAlloc sometimes does not return colliders that have moved and do not have rigidbodies.
- On exiting playmode may throw exceptions occasionally due to order in which objects are destroyed.
- Highlight shader does not work on Mac (Metal).

## Copyright

Copyright (c) 2022 VRChat
See License.md for full usage information

## Credits

Based on [CyanEmu](https://github.com/CyanLaser/CyanEmu) by [CyanLaser](https://github.com/CyanLaser), who also made this version.
