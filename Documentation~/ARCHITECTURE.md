# Implementation and local outputs

English | [简体中文](ARCHITECTURE.zh-CN.md)

Editor-only code generates FX animation, persistent scan materials, MA integration, and optional SDK audio behaviors. No uploaded custom MonoBehaviour is required.

- KitRecipe/ScanSetupData contain explicit local bindings and settings. `core.json` provides effect timings with empty outfit/body/audio lists; `plum-verified.json` is an optional adapter preset.
- KitBindings.Configure stores user-selected object paths and an imported controller path without moving objects. FitScanRange estimates top/start/bottom from prepared geometry. KitSetupWindow exposes these operations for manual installation.
- KitInstaller creates a disposable copy and an independent `Assets/ScanKitLocal/Generated_<id>` directory. Registry.asset records ownership. Validation precedes switching copies; the source is preserved inactive. Update regenerates from the source; uninstall removes only owned copies/directories and retains recipes/products. One installation per project.
- ScanBuilder/ScanMotion generate four outfit states and the shared halo/clip plane. Visual duration remains 3.65s, with 1.2s rise, 1s expansion, and 1.1s scan. Audio adds an independent optional prelude; silent transformation does not wait for it.
- Scan materials are local persistent assets. lilToon original properties and idle materials are retained. Non-lilToon conversion is rejected by default; the explicitly enabled historical fallback preserves only selected basic properties.
- ScanBodyMasks handles explicitly configured shapes on one body. It transfers matching MA rules on the copy and restores visible body geometry during the sweep. It does not reset unrelated body shape settings or reconstruct deleted source geometry.
- ScanHaloDefaults copies the user's original Angelring controller locally, removes known adjustment layers/menu/parameters, and keeps default particles. Halo mesh/material derivatives are generated locally, never distributed.
- ScanBuildPreparation enables controlled outfits before preprocessing; DressingTools-specific conflict removal is opt-in. The Play lifecycle bridge is installed only if DressingFramework is available. Post-binding calibration derives A/B ownership from authored curve direction, uses common Hips space, and accommodates moved animation paths without outfit-name suffixes.
- BOutfitWingPreparation is opt-in. It uses persistent local scan materials after MA binding and before shader optimization. Core validation does not require wings; enabled adapter validation uses actual renderer counts.
- KitValidation performs preflight, generated curve/material checks, local preprocessing, and Prefab save/reload checks. When DressingFramework is installed, its version-specific builder is invoked at the SDK callback slot because its adapter rejects non-SDK call stacks. This is a preprocessing simulation, not a `.vrca` build or upload.

Core uses one synchronized Bool, KAT/OutfitB. Configured audio adds KAT/Sound (a second Bool). Original avatar/product parameters count separately.
