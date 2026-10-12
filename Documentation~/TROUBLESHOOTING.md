# Troubleshooting

English | [简体中文](TROUBLESHOOTING.zh-CN.md)

| Symptom | Checks and action |
| --- | --- |
| Compilation errors / missing assemblies | Install dependencies from the version table first. Do not treat a generated project directory as the package. Check for duplicate installations. |
| Preflight rejects versions | Use the recorded versions, or validate new versions in a separate test project. Do not silently disable checks and claim support. |
| Transformation Scan already exists | Automatic migration of the legacy implementation is not supported. Preserve the original project and prepare a dressed source avatar without the transformation module. |
| Gesture Manager finds no avatar / avatar disappears in Play mode | Inspect the first SDK/DressingTools error. Confirm that Play lifecycle preparation is installed and the source and copy are not both participating in the build. Adding another Descriptor does not resolve a build failure. |
| Wings turn magenta during scanning but look normal at rest | Check that generated wing materials are persistent assets, that their PPtr references survive preprocessing, and that shaders compile. Run local validation and the shader compatibility checks. |
| Missing body regions | Check the four preset Body_base BlendShapes and MA Shape Changer. Do not clear all body-shape BlendShapes globally. |
| New materials are absent from the scan | Change the source avatar's original materials, then run Update. Editing only generated scan materials is insufficient. |
| Volume changes have no effect | Change recipe.settings.audioVolume and run Update. Volume is also animated, so an AudioSource-only edit will be overwritten. |
| No sound | An empty audioPath produces a silent version. Check the local AudioClip, KAT/Sound, and prelude state. Reverse transformations intentionally do not trigger audio. |
| Expressions do not reset | Check VRCFury's Write Defaults Auto feature and build errors. Do not indiscriminately switch every layer on the avatar to WD Off. |
| Uninstall cannot find the avatar | Open the installation scene recorded in the Registry. Avoid manually moving or deleting registered copies. Do not delete the source avatar or the entire Assets directory. |

When a check fails, retain the first Console exception, version information, and local validation report for diagnosis. Remove personal paths and product binaries before sharing them.
