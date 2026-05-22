# Spline Mesher
*by Staggart Creations*

## Project Requirements

**Requires the following packages**
- Splines
- Mathematics

## New editor options
- Spline Mesher component
- Context menu on Mesh Filter component to convert it into a spline
- Context menu on Spline Container component to add a Spline Mesher
- New menu option: GameObject/3D Object/Spline Mesh

## Runtime usage

There is no editor-only code in place, so runtime usage is possible. That's not to say that a complete UI and control system is in place.
You can call the Rebuild() function on a Spliner Mesher component from any external script.

Note that the input mesh needs to have Read/Write enabled for this to work!