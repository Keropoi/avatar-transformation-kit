# Preparation and pinned versions

English | [简体中文](COMPATIBILITY.zh-CN.md)

## Tested baseline

| Component | Version in the tested project |
| --- | --- |
| Unity / platform | 2022.3.22f1 / Windows PC Avatar |
| VRChat Avatars + Base | 3.10.1 |
| Modular Avatar | 1.16.2 |
| NDMF | 1.11.0 |
| VRCFury | 1.1336.0 |
| lilToon | 2.3.2, with the narrowly scoped compatibility preparation below |
| Poiyomi Toon / ThryEditor (original sword material) | 10.0.24 / 2.74.3 |
| DressingTools | 2.6.0 |
| DressingFramework | 2.1.1 |
| Chocopoi AvatarLib | 1.1.1 |
| Gesture Manager (local testing) | 3.9.7 |
| MCP for Unity (Agent bridge) | 10.3.1-beta.7; commit `142ea0af4890337f0ff6658df61db23982313717` |

This table records the tested environment, not the latest available versions. By default, the installer requires matching core dependency versions. Quest/Android, arbitrary product substitutions, other body shapes, and other versions are outside this preview's verified scope.

## Asset preparation

- The avatar must be a root object in a saved scene, with a valid Descriptor and its original expression/menu/outfit configuration. It must not already contain the legacy transformation module.
- The preset requires zero rotation and unit scale on the avatar root. Preserve the prepared assets' internal bone scales.
- Fit, rig, and adjust outfits A and B according to their product instructions. See `Presets/plum-verified.json` for the preset's top-level group names.
- Place the halo and `AngelNeonWing(B)` under the avatar at the desired positions. The wing's MA Bone Proxy must target Chest and preserve its world pose. Do not ask the Agent to guess how products should be fitted.
- The source halo's MA Merge Animator must remain usable. The generator removes its adjustment menu on the installed copy and preserves default particles.
- `Body_base` must have Shoulder_OFF, Chest_2_OFF, Spine_1_OFF, and Ankle_OFF. The preset uses A=100/B=0; other body-shape BlendShapes are not forcibly reset.
- Original product toggles, such as the sword and bag controls, must already work. The generator changes activation and clipping of controlled outfits on the copy; it does not replace the original expression controller.
- Audio is an optional, locally available AudioClip that the user has permission to use. Current reproduction settings are audioLeadSeconds=0.7 and audioVolume=0.65.

## lilToon 2.3.2 compatibility preparation

In this baseline, shader optimization treated commented include lines as active includes, causing a `CBUFFER_START` compilation error. See the related upstream report, [lilToon issue #366](https://github.com/lilxyzw/lilToon/issues/366). The narrowly scoped workaround used in this project removes three **comment lines** beginning with `//#include` from `CustomShaderResources/Misc/ReferenceUVs.lilblock`. They reference `Includes/lil_pipeline_brp.hlsl`, `Includes/lil_common.hlsl`, and `Includes/lil_common_appdata.hlsl`. Do not remove active includes.

This applies to the specific 2.3.2 file structure, not to future versions generally. Back up the original file and verify the version and all three comment lines before editing. Stop and report if they do not match. The kit neither distributes modified third-party shaders nor automatically rewrites Packages. Once this preparation is complete, the Agent can proceed with installation. First-install Preflight checks for the remaining commented includes; revalidate after shader updates.

## Package format

The kit uses a Unity package directory structure. Generated data goes under Assets, not into the read-only package directory. This release provides a source package; a VCC automatic-install repository index is not yet available. `vpmDependencies` records compatible dependencies, but importing a local directory does not install them. Install dependencies through VCC first. The format follows the [official VPM documentation](https://vcc.docs.vrchat.com/vpm/packages/).
