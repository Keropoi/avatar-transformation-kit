# Avatar Transformation Scan Kit

English | [简体中文](README.zh-CN.md)

An Editor toolkit for a halo-driven, complementary A/B outfit transformation on prepared VRChat avatars. Current preview: `0.2.0-preview.1`.

**You choose and prepare outfits A and B. The kit supplies binding tools and animation generation.** The initial goal is one Agent prompt for installation and local validation when the documented prerequisites are met. Outfit fitting, arbitrary shader conversion, and VRChat upload are outside the installation workflow.

## Core effect

- Halo rises for 1.2 seconds, then expands for 1 second; both stages start fast and ease out.
- Descending scan lasts 1.1 seconds with a pronounced slow–fast–slow curve. A disappears below the same plane that reveals B above it.
- The kit restores original outfit materials at rest, preserves the source avatar, and supports update and scoped uninstall.
- No fixed outfit names, Plum body, sword, or neon wings are required. Core scan materials must support lilToon's plane dissolve.
- Body anti-clipping masks and forward-only audio are optional local configuration. Audio defaults: 0.7-second lead, volume 0.65; a separate sound toggle is generated only when a clip is supplied.
- Uses VRCFury Write Defaults Auto on the installed copy. No body poses or camera animations are generated.

## External halo — purchased by each user

This release uses the **Angelring Halo / comet_shop** integration. Each user purchases and imports the corresponding product, positions it under their avatar, and supplies the original halo FX controller. No halo meshes, textures, materials, animations, particles, or product-derived assets are distributed here. They are processed locally in the consuming Unity project.

The repository author plans to obtain the product creator's authorization. **That authorization has not yet been documented in this repository.** Do not interpret this preview as a claim that permission has already been granted. The official product URL and tested download version remain author-supplied details in the [asset list](Documentation~/BOOTH_ASSETS.md).

## Install with an Agent

1. Install the [core dependencies and prepare the avatar](Documentation~/COMPATIBILITY.md). Dress and rig both outfits before using the kit. Import and position your purchased halo. Save the scene.
2. Place the package in `Packages/com.keropoi.avatar-transformation/`. Do not install a second copy under Assets. Optional adapter dependencies are needed only for the adapters you use.
3. Select the prepared source avatar, connect Unity MCP, and give the Agent the actual names of your A/B groups:

> First read Packages/com.keropoi.avatar-transformation/Documentation~/AGENT_SETUP.md. My avatar and purchased Angelring Halo are prepared. Outfit A is [object names], and outfit B is [object names]. Install the halo scan transformation and complete local validation. Report missing prerequisites and untested items.

Replace the bracketed text with scene object names. The Agent creates a local core recipe, binds your objects, checks materials and body-mask rules, installs on a copy, and reports validation results. It must not guess which garments belong to A or B.

For manual installation, open `Tools/ScanKit/Setup User Outfits`. Assign source avatar, recipe, hips, A/B groups, source halo, visual child, and original halo FX. Save bindings, configure body masks if needed, check scan heights/size, run Preflight, then install and validate.

## Core + optional adapters

| Feature | Core default | Optional configuration |
| --- | --- | --- |
| A/B outfits | User-supplied, non-overlapping groups; lilToon materials | Optional Plum reproduction preset |
| Halo | External purchased Angelring Halo | This is the supported halo integration, not a bundled asset |
| Body masks | Empty; no Plum-specific BlendShapes required | Explicit body path and A/B values; destructive deletion rules must be handled |
| Neon wings | Not required | Ordinary supported B group, or the dedicated wing adapter; never both |
| DressingTools | Adapter disabled | Enable for prepared DressingTools outfits; requires its recorded dependencies |
| Non-lilToon materials | Rejected | Plum reproduction's explicitly enabled simplified conversion; not a general shader adapter |
| Sound | Silent when no clip is supplied | Local authorized AudioClip with forward-only playback and independent toggle |

See [ADAPTERS.md](Documentation~/ADAPTERS.md) for configuration. The [Plum asset list](Documentation~/PLUM_ADAPTER_ASSETS.md) describes the optional historical combination and is not a required shopping list for the core.

## Update and uninstall

Edit the **source avatar** or local Recipe, then run `Tools/ScanKit/3 Update from Source Avatar`. A replacement copy is generated and checked before switching. Edits made directly to the installed copy do not survive updates. Previous generated directories remain until uninstall.

`Tools/ScanKit/5 Uninstall` removes registered copies and owned output directories, restores the source's prior active state, and retains imported products and recipes. Save the scene after every operation. This preview manages one installation per project.

## Documentation and verification

- [Agent workflow](Documentation~/AGENT_SETUP.md)
- [Required halo and user-supplied assets](Documentation~/BOOTH_ASSETS.md)
- [Preparation and compatibility](Documentation~/COMPATIBILITY.md)
- [Optional adapters](Documentation~/ADAPTERS.md)
- [Implementation](Documentation~/ARCHITECTURE.md)
- [Troubleshooting](Documentation~/TROUBLESHOOTING.md)
- [0.2 validation record and limits](Documentation~/VALIDATION.md)

The code no longer requires the original outfit combination. Verification is still limited to the recorded local fixtures and versions; it does not establish compatibility with every avatar, shader, or outfit. Local preprocessing is not a VRChat build or in-game acceptance test.

Public source preview; **no open-source license granted**. See [LICENSE](LICENSE). Product licenses and any future creator authorization are separate from the code's license.
