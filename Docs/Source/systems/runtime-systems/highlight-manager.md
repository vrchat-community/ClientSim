---
uid: highlight-manager
---

# HighlightManager

The HighlightManager will take an object and display an outline highlight effect for that object. This system wraps VRChat’s HighlightFX class. HighlightFX only takes a single renderer, whereas the HighlightManager takes a GameObject. Matching how VRChat handles highlighting objects, all renderers on the object and on its children are used for highlighting. Renderers that are disabled, have a null mesh, or are part of a static batch are ignored. If an object has no valid renderers, then a Highlight Proxy is used based on the first collider on the object. The Highlight Proxy will copy the transform values of the original mesh and also apply the collider size and scale to make it appear that the collider is being highlighted. The HighlightManager is used to visualize the results from the [PlayerRaycaster](xref:player#playerraycaster) system. There is no set limit to the number of objects that can be highlighted, but only 2 objects are expected to be highlighted at once through ClientSim, one object per player hand. The HighlightManager links to the VRCSDK API for InputManager.EnableObjectHighlight. This hook only takes in renderers though and does not go through the full steps of finding children objects and creating proxies. 

> [!NOTE]
> The HighlightManager currently shows only a preview for how the object will look on Quest. A style matching Windows systems is forthcoming.