# Settings Window

The Settings window displays all the [ClientSim Settings](../runtime/settings.md) that can be edited. Some of these values cannot be changed at runtime. There is also a button to spawn remote players and a text field to give those remote players a custom name.
The Settings window can be opened through the “VRChat SDK/Utilities/ClientSim” option.

## Project Setting Warnings

If the Unity Project Settings do not match what is needed to run ClientSim, warnings will display for each project setting that is invalid. 

## Project Settings Setup

ClientSim requires specific Unity Project settings to run properly. These settings are not default for a project and require action from the user to modify. The ClientSim Settings Window will display the current incorrect project settings with buttons to update the project to the correct setting. This behaviour differs from CyanEmu, which on every assembly reload would automatically force update project settings.

### Input Manager settings are incorrect
ClientSim requires the new Unity Input System. Upon importing this package, it will ask the user to enable it and restart Unity. Neither option in this dialog is the correct setting for ClientSim and the user must enable both old input and new input. Clicking the “Do it!” button in the settings window will set the correct input and force a Unity restart.

### Input Axes differ from VRChat’s
While not used within ClientSim, this is helpful for users to test getting input within Udon Programs.

### Audio Spatializer not set
ClientSim depends on the Oculus Audio Spatializer.

### Project Layers and Collision Matrix not set
The user must open the VRChat Build Control Panel to properly set this.