# InteractiveLayerProvider

The InteractiveLayerProvider simply listens to menu open state events and provides a layer mask for which layers are currently interactive. When the menu is open, only the UI and UIMenu layers are interactive. When the menu is closed, all other layers, excluding MirrorReflection, are interactive. InteractiveLayerProvider is used by [Raycasters](player.md#raycaster) and the [ClientSimInputModule](input.md).
