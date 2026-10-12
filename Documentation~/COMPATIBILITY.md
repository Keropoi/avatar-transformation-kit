# Preparation and compatibility

English | [简体中文](COMPATIBILITY.zh-CN.md)

## Recorded baseline

These are tested project versions, not a statement of the newest releases. Strict version checks remain enabled by default.

| Core component | Recorded version |
| --- | --- |
| Unity / platform | 2022.3.22f1 / Windows PC Avatar |
| VRChat Avatars + Base | 3.10.1 |
| Modular Avatar | 1.16.2 |
| NDMF | 1.11.0 |
| VRCFury | 1.1336.0 |
| lilToon | 2.3.2 with preparation below |

| Optional adapter / testing component | Recorded version |
| --- | --- |
| DressingTools / DressingFramework / Chocopoi AvatarLib | 2.6.0 / 2.1.1 / 1.1.1 |
| Poiyomi Toon / ThryEditor (historical sword) | 10.0.24 / 2.74.3 |
| Gesture Manager (local testing) | 3.9.7 |
| MCP for Unity (Agent bridge) | 10.3.1-beta.7; commit `142ea0af4890337f0ff6658df61db23982313717` |

DressingTools versions are required only with its adapter; the optional Plum preset checks the full historical baseline, including Poiyomi. Core package dependencies exclude these optional packages. Current tests ran in the author's project, which already contains optional plugins; no fresh minimal-dependency project or Android/Quest build was tested.

## Prepared inputs

- A saved scene-root VRCAvatarDescriptor with valid original expression/rig settings. Avatar root rotation zero and scale one. No pre-existing transformation module.
- User-selected, already fitted/rigged A/B groups. Both sets are nonempty; groups must be distinct and not nested. No specific outfit names or product combination required. Shared clothing stays outside the groups.
- Enabled outfit mesh renderers with lilToon plane-dissolve materials. Unsupported shaders fail preflight; simplified conversion is explicit optional configuration, not a general compatibility guarantee.
- Skinned outfits must resolve to the avatar's hips or descendants after dressing/binding. Post-build calibration uses Hips as the common scan space without replacing skinning bones.
- User-purchased Angelring Halo positioned under the avatar, with its original MA Merge Animator, visual child, and original FX controller. The adapter removes its adjustment menu on the copy while retaining default particles; it still targets this product's layer/particle structure.
- Check local scan heights and expandedScale. FitScanRange estimates heights from clothing bounds; it does not automatically fit the halo radius or verify visual clearance.
- Body masks may be empty when no mask coordination is needed. Otherwise set a bodyPath and explicit BlendShape rules. Destructive MA Shape Changer Delete rules on selected outfits must be mapped before installation. The kit cannot reconstruct permanently removed mesh geometry.
- Original accessory and expression toggles should work before installation. Competing clothing toggles may require explicit adaptation; the kit does not promise automatic resolution of arbitrary controllers.
- Optional authorized local AudioClip; no sound is installed when audioPath is empty. Optional wings and adapters are described in [ADAPTERS.md](ADAPTERS.md).

## lilToon 2.3.2 compatibility preparation

In this baseline, shader optimization treated commented include lines as active includes, causing a `CBUFFER_START` compilation error. See the related upstream report, [lilToon issue #366](https://github.com/lilxyzw/lilToon/issues/366). The narrowly scoped workaround used in this project removes three **comment lines** beginning with `//#include` from `CustomShaderResources/Misc/ReferenceUVs.lilblock`. They reference `Includes/lil_pipeline_brp.hlsl`, `Includes/lil_common.hlsl`, and `Includes/lil_common_appdata.hlsl`. Do not remove active includes.

This applies to the specific 2.3.2 file structure, not to future versions generally. Back up the original file and verify the version and all three comment lines before editing. Stop and report if they do not match. The kit neither distributes modified third-party shaders nor automatically rewrites Packages. Once this preparation is complete, the Agent can proceed with installation. First-install Preflight checks for the remaining commented includes; revalidate after shader updates.

## Distribution

Source package content goes in `Packages/com.keropoi.avatar-transformation/`; generated files go under Assets. Dependencies must be installed separately through VCC or their supported workflow. No VCC repository index is provided yet. Package format follows the [official VPM documentation](https://vcc.docs.vrchat.com/vpm/packages/). Creator authorization for the external halo is pending documentation; no product assets are distributed.
