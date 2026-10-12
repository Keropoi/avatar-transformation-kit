# Avatar Transformation Scan Kit

English | [简体中文](README.zh-CN.md)

A transformation toolkit for prepared Plum VRChat avatars. Current package version: `0.1.0-preview.1`.

The initial goal is **one user prompt for an Agent to install the effect and perform local validation, given the verified asset combination, plugin versions, and preparation steps.** This preview includes a generator, a Plum preset, install/update/uninstall/validation entry points, and Agent instructions. Purchasing assets, fitting arbitrary outfits, and uploading to VRChat are outside its scope.

## Effects

- The halo rises for 1.2 seconds, then expands for 1 second. Both stages start fast and ease out.
- The descending scan lasts 1.1 seconds, with a pronounced slow–fast–slow curve and complementary clipping of outfits A and B.
- Outfit B's wings appear progressively during the scan and return to their original materials at rest. The great sword remains exclusive to B and retains its original toggle.
- Optional audio starts 0.7 seconds before the forward visual transformation, at volume 0.65. It has a separate toggle and does not play on the reverse transformation. Without an audio configuration, installation produces a silent transformation.
- Body anti-clipping BlendShapes are coordinated with the outfit change. The halo product's adjustment menu is removed while its default particles are retained.
- Uses VRCFury's Write Defaults Auto fix. No body poses or camera animations are added.

## Preparation and installation

1. Purchase and import your own assets using the [draft Booth asset list](Documentation~/BOOTH_ASSETS.md). Product files, derived materials/meshes/controllers, and audio are not included in this repository.
2. Follow the [preparation and version requirements](Documentation~/COMPATIBILITY.md) for a Windows avatar project: fit and rig the outfits, position the halo and wings, and name the groups according to the preset. Save the scene.
3. Download this repository's package and place its contents in `Packages/com.keropoi.avatar-transformation/`. Install dependencies first. Do not install duplicate copies under both Assets and Packages.
4. With Unity MCP connected, send your Agent this prompt:

> First read Packages/com.keropoi.avatar-transformation/Documentation~/AGENT_SETUP.md. The assets and plugins are prepared according to the Plum preset. Install the transformation effect on the currently selected avatar and complete local validation; report any missing prerequisites precisely.

The Agent checks prerequisites, creates a local recipe and avatar copy, runs installation and SDK preprocessing checks, and reports the scope of validation. The `Tools/ScanKit` menu also supports manual operation.

## Updating and uninstalling

Installation creates an avatar copy. On success, the original avatar is deactivated. This preview manages one installation per project.

Edit the **source avatar's** materials or preparation, or the local Recipe, then run `3 Update from Source Avatar`. The system generates and checks a new copy before replacing the previous one. Edits made directly to the installed copy are not preserved during updates. Previous output directories are retained for traceability until uninstall.

`5 Uninstall` removes the kit-managed copy and generated directories, and restores the source avatar's previous active state. Purchased assets and the Recipe are retained. Save the scene after installing, updating, or uninstalling.

## Documentation and current limits

- [Agent installation workflow](Documentation~/AGENT_SETUP.md)
- [Booth asset list — author details pending](Documentation~/BOOTH_ASSETS.md)
- [Versions, preparation, and compatibility](Documentation~/COMPATIBILITY.md)
- [Configuration, implementation, and validation scope](Documentation~/ARCHITECTURE.md)
- [Troubleshooting](Documentation~/TROUBLESHOOTING.md)
- [Validation record](Documentation~/VALIDATION.md)

Local validation is not verification in VRChat after upload. Other product versions, body shapes, shaders, and outfit structures are outside the verified support scope. The code is public for inspection, but **no open-source license has been granted**. See [LICENSE](LICENSE).
