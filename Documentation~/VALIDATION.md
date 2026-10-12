# Validation record: 0.2.0-preview.1

English | [简体中文](VALIDATION.zh-CN.md)

Date: 2026-10-12. Version baseline: [COMPATIBILITY.md](COMPATIBILITY.md). Historical results: [0.1 record](VALIDATION_0.1.md).

## Isolation

Testing used new, local scenes separate from the original backup. The original backup scene was unloaded during core testing. Its scene/meta and 165 initial transformation files were recorded and verified by SHA-256 (167 protected files total). Local test scenes, screenshots, purchased materials, meshes, controllers, and generated output are excluded from distribution.

## Checks performed

| Check | Result |
| --- | --- |
| Editor compilation of the core package | Passed |
| Explicit binding API and core installation with renamed A/B groups | Passed; 30 outfit scan renderers, no sword, no wing adapter, no audio |
| Core fixture with separate user-named procedural A/B meshes | Installation and local preprocessing passed; two renderers, empty body masks, no wing/audio/DressingTools adapter |
| Body-mask migration from a clothing container | Passed on a temporary copy; source rules preserved |
| Post-build scan-plane alignment | Passed for core and optional wing renderers at scan start/midpoint/end |
| Height-fitting API | Passed on the procedural fixture; heights estimated from its geometry |
| Update and scoped uninstall | Passed; source/recipe retained and owned outputs removed |
| Duplicate/overlapping A/B rejection and preservation of current installation | Passed |
| Unsupported Standard shader rejection and preservation of current installation | Passed |
| Exact reverse curves / 1.1s descending easing | Passed; first quarter travels 6.25% of the distance |
| Local preprocessing and serialized material references | Passed on core and optional Plum fixtures; Prefab save/reload included |
| Optional Plum + separate-wing + audio adapters | Passed; 31 outfit renderers, four wing-height bindings, 0.7s audio lead, volume 0.65, no reverse audio trigger |
| Actual processed FX controller sampled in a fixed full-body view | A idle, forward scan, B idle, reverse scan, and A idle checked; scan materials restore at rest; no apparent missing body regions or magenta errors in inspected frames |
| Play mode and Gesture Manager | Avatar detected and linked; A→B→A completed |
| Sample expression reset in Play | GestureLeft 4→0 and Facial_Option_heart on→off; Option_eye_heart returned from 100 to 0 |

Static FX checks used the actual processed controller on a temporary copy, with its humanoid Avatar binding disabled to isolate material behavior. They are separate from the Play/Gesture Manager round trip. No AnimationClip.SampleAnimation-only result is presented as runtime evidence.

## Limits

- Tests used the author's existing prepared avatar/project, including a procedural outfit fixture. This is not a fresh import from an empty project or a second independently prepared avatar.
- Optional plugins were already installed. Disabling kit adapters in a fixture does not prove installation in a project containing only minimal core dependencies.
- Structural/local preprocessing and Prefab reload checks are not a real SDK .vrca build, upload, or VRChat game test.
- Only one gesture and one FacialOption were sampled. The complete gesture/menu matrix, game network sync, full-body tracking, extreme poses, dynamic particles, perceived audio, and arbitrary shader/outfit combinations remain unverified.
- Creator authorization for the external halo and its exact official product link/download version remain author-supplied details. No completed authorization is claimed.
- Existing tests establish the recorded fixtures and plugin baseline, not universal compatibility. Core APIs accept other explicit bindings under the documented material/rig contract; those combinations need their own acceptance checks.
