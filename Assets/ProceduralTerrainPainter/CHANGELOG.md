1.0.5

Added:
- Direction modifier: Projects a layer based on a given XY rotation. Option to use the main directional light.
- C# API: RepaintAll and RepaintTerrain functions have an optional bool parameter, to sync the GPU splatmap data to the CPU/disk.

Changed:
- Demo scene is now set up for URP by default
- Better handling of asynchronous saving of terrain painting data.
- 
Fixed:
- Error in Unity 6+ regarding missing icon when selecting a Terrain Painter
- Warnings regarding "'R8_SRGB' is not supported".
- Auto repaint functionality also causing Terrain Painter components to repaint, even if they aren't targeting the terrain being edited.

1.0.4

Added:
- Slider in the UI to expand the layers list, making all of them visible.

Changed:
- Improved Gradient noise modifier, now guaranteed to provide a smooth gradient
- Improved error handling when any assign terrain layer assets went missing

Fixed:
- Noise modifier only seeming to have an effect when the tiling value was around 100.

1.0.3

Changed:
- When closing the Terrain Painter inspector, the project will now be saved. This ensures the terrain data is saved to disk. Avoids any errors from the PaintContext.ApplyDelayedActions function
- Asset now uses an assembly definition.
- Minimum supported version of MicroSplat is now 3.8.1

Fixed:
- Errors in 2021.2.0a15 due to the TerrainAPI leaving the experimental namespace

1.0.2

Added:
- Callback event for when a terrain is repainted (TerrainPainter.OnTerrainRepaint)
- A warning is now displayed if any terrain references have gone missing

Changed:
- Adding a new modifier now automatically selects it

Fixed:
- Texture modifier not aligning correctly if a terrain had a large negative position
- Modifier settings not drawing when Odin is installed

1.0.1

Added:
- Auto repaint option in Settings tab. Repaints a terrain when its height is modified.
- Option to refresh Vegetation Studio Pro when terrain is repainted

Changed:
- Renamed namespace to "sc.terrain.proceduralpainter" for consistency between other terrain tools

Fixed:
- Heatmap preview highlighting wrong terrain layer in some cases

1.0.0
Initial release