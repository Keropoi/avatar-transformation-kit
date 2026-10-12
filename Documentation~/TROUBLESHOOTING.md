# Troubleshooting

English | [简体中文](TROUBLESHOOTING.zh-CN.md)

| Symptom | Action |
| --- | --- |
| Empty/missing bindings | Explicitly select both outfit sets, hips, imported halo/visual, and original controller. No fixed outfit names are required. |
| Overlapping groups | Keep A/B groups distinct and non-nested; shared clothes stay outside. |
| Unsupported material | Core requires lilToon plane dissolve. Do not silently convert arbitrary shaders; choose supported materials or a separately tested explicit adapter. |
| Version/assembly errors | Install the core baseline; add optional dependencies only for enabled adapters. Do not duplicate package installations or silently disable checks. |
| Existing transformation | Preserve the legacy scene and prepare a separate source. Automatic migration is not supported. |
| Missing body / destructive shape-rule rejection | Map bodyPath/bodyMasks explicitly, including Delete rules. Do not clear all BlendShapes. Permanently deleted geometry needs restoration in source assets. |
| Misaligned scan / too-small halo | Verify heights, expandedScale, root transform, and post-binding hips space. Bounds fitting only estimates heights; inspect the result. |
| Gesture Manager finds no avatar / disappearance | Inspect the first build error and optional dressing setup. Ensure source and installed copy are not both active. Adding a Descriptor does not resolve a preprocessing failure. |
| Magenta material during scan | Run shader compatibility checks, verify persistent generated material references, and run local validation. |
| New source materials not reflected | Edit the source, then Update; do not edit generated scan materials alone. |
| Volume edits overwritten / no sound | Change recipe.settings.audioVolume and Update. Empty audioPath is silent; check imported AudioClip and KAT/Sound. Reverse playback is intentionally silent. |
| Wings absent | Core does not automatically add wings. Select a compatible B group or configure the optional wing adapter, not both. |
| Expressions do not reset | Check VRCFury Write Defaults Auto and build errors; do not switch all avatar layers indiscriminately to WD Off. |
| Uninstall cannot resolve avatar | Load the registered scene. Do not delete the source or the entire Assets folder. |

Retain the first Console exception, version list, and local validation report. Remove personal paths and product binaries before sharing. See [VALIDATION.md](VALIDATION.md) for tested scope.
