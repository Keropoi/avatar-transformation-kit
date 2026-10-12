# Validation record: 0.1.0-preview.1

English | [简体中文](VALIDATION_0.1.zh-CN.md)

Date: 2026-10-12. See [COMPATIBILITY.md](COMPATIBILITY.md) for environment versions.

## Checks performed

| Check | Result |
| --- | --- |
| Package compiles in Unity and loads its separate Editor assembly | Passed |
| Installation creates a separate copy of a prepared source avatar and preserves the source | Passed; 31 outfit scan renderers |
| Update replaces the old copy without leaving two active avatars | Passed |
| Invalid timing configuration is rejected while preserving the existing installation | Passed |
| Uninstall removes registered copies/output directories, restores the source, and retains the Recipe | Passed with both audio and silent installations |
| Corresponding forward/reverse curves and the pronounced slow–fast–slow 1.1-second descending scan | Passed |
| Audio branch has a 0.7-second prelude, volume 0.65, and no reverse trigger | State-machine and curve checks passed; no listening assessment performed |
| Configuration with no audio supplied | Installation and local preprocessing passed; no AudioSource or sound parameter |
| DressingFramework adapter and simulation of SDK-registered callback order | Passed with both audio and silent configurations |
| Wing scan curves after preprocessing | Passed; two wing renderers × two transformation directions = four scan-height curves |
| All outfit material references and shader error checks | Passed; no transient materials left as animation references |
| FX material references after saving and reloading a local Prefab | Passed |
| Manually advancing the actual processed FX controller through A→B→A | Passed; outfit and wing visibility reset |
| Fixed full-body view at the start, scan midpoint, and B idle | Local static samples inspected; outfit scanning and white wing materials appeared correct, with no magenta error materials |
| Release file allowlist | Passed; only source, numeric/path-binding presets, documentation, and their own metadata |

FX testing advanced the actual FX controller on a temporary copy. That copy's humanoid Avatar binding was disabled to isolate material checks. Full-body tracking and game network synchronization were not simulated. Calling AnimationClip.SampleAnimation alone can reset the humanoid pose or fail to reproduce material animation; direct sampling is not proof of runtime behavior.

## Limits

- The test source came from a copy of the author's already dressed project. The legacy transformation module was removed and original materials/halo controller restored on the test copy. This was not an independent reproduction from an empty project with freshly downloaded and imported products.
- This validation round did not repeat the complete Gesture Manager acceptance flow for expression menus and sword interactions, or upload the package-generated avatar to VRChat. Compatibility handling from the existing implementation was migrated, but this package version still needs those checks.
- The local validation entry point simulates preprocessing and saves/reloads a Prefab for checks. It does not generate or reload a `.vrca`, or upload anything.
- Booth product versions and official links still need to be completed by the author. The one-prompt installation goal applies to the already prepared combination described in the documentation, not to arbitrary new users or asset environments without manual preparation.
- Extreme-pose clipping, full-body tracking poses, particle dynamics, perceived runtime spatial audio, and other shader/product versions are not verified.

All test screenshots, scenes, and generated assets remain local and are not published with the repository.
