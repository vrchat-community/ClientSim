# Settings

The ClientSim Settings are not a system, but data on how to run ClientSim.
### Enable ClientSim
Should ClientSim be enabled when entering playmode? ClientSim is forced disabled when uploading worlds.

### Enable Console Logging
Should Debug information be logged to the console?

### Remove “EditorOnly”
On enter playmode, should all objects tagged with “EditorOnly” be deleted?

### Set Target FrameRate
Should ClientSim set the Application target framerate?

### Target FrameRate
The expected framerate for Unity while in playmode. This will set both Application.TargetFramerate and the FixedTimeDelta.

### Startup Delay
How long should ClientSim wait before starting and initializing Udon? Use this as a way to simulate long world loading times and verify Unity component behavior.

### Spawn Player Controller
Spawn a controllable player when starting ClientSim. If disabled, a local player is still created to prevent Udon Programs crashing.

### Show Desktop Reticle
Should the desktop reticle be displayed or not?

### Show Tooltips
Show tooltips above interactable objects

### Invert Mouse Look
Should the mouse Y be inverted

### Player Height
The height of the player in unity units. This is clamped between 0.2 and 80.

### Local Player Name
What is the name of the local player, used for VRCPlayerApi.displayName

### Local Player Is Master
When set to false, a remote player is spawned and set as master.

### Is Instance Owner
Is the local player the instance owner?
