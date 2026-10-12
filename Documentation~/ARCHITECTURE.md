# Implementation and local files

English | [简体中文](ARCHITECTURE.zh-CN.md)

The package contains Editor code only. Runtime behavior is generated into the FX Animator, standard animation curves, materials, and SDK audio StateMachineBehaviours. It does not require uploading a custom MonoBehaviour.

`KitRecipe.settings` configures bindings, timing, audio, and body masks. `Presets/plum-verified.json` supplies initial values. Create Recipe creates a local ScriptableObject; subsequent calls preserve existing configuration.

`KitInstaller` maintains `Assets/ScanKitLocal/Registry.asset`, which records the source avatar, installed copy, and owned output directories. Each generation uses a separate `Generated_<id>` directory and validates the new copy before switching to it. Transformation components are not written onto the source avatar. Only one installation per project is supported; blind replacement across scenes is disallowed.

`ScanBuilder/ScanMotion` generates four outfit states, complementary material clipping, and halo curves. The preset visual duration is 3.65 seconds; clip frame quantization may slightly affect the actual length. The audio prelude is independent of the visual curves. The silent branch does not wait for a prelude.

`ScanBodyMasks` migrates only the body-clipping rules explicitly listed in the preset, preventing permanent body removal when both outfits are enabled. `ScanHaloDefaults` makes a local copy of the original halo FX controller and removes adjustment layers and parameters.

`ScanBuildPreparation` enables the A/B groups before DressingTools processes them and removes conflicting cabinet-anim modules. In Play mode, the same preparation runs early through the verified DressingFramework lifecycle API. After dressing, `ScanBuildCalibration` recalibrates each renderer's clipping coordinates and converts the static sword to a single-bone skinned mesh to keep coordinates consistent while moving.

`BOutfitWingPreparation` adds the wings to the scan after MA binding and before lilToon's material scan, preserving skinning bones and original materials. Scan materials are saved as persistent `.mat` assets to prevent references from disappearing in the uploaded bundle despite appearing correct in the Editor. Idle states restore the product's original materials.

`KitValidation` checks versions, bindings, BlendShapes, timing, numerical scan-plane alignment, asset persistence, and shader compilation. Local build validation runs callbacks in SDK registration order on a temporary copy and checks processed paths and wing curves. DressingFramework's SDK entry checks the call stack and ignores non-SDK calls, so local validation uses that version's AvatarBuilder entry point for the dressing stage. This is a local preprocessing simulation: it does not produce a `.vrca`, replace actual SDK packaging, visual checks, or in-game validation, or automatically upload anything.

The outfit Bool parameter is `KAT/OutfitB`; optional audio uses `KAT/Sound`. Reserve space for two synchronized Bools. Account for the original products' and avatar's other parameter costs separately.
