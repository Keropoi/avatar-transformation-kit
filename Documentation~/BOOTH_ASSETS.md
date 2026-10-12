# Core asset checklist

English | [简体中文](BOOTH_ASSETS.zh-CN.md)

Core users choose their own avatar and outfits. This repository does not distribute any purchased product, audio, or locally generated derivative.

| Asset | Required? | Preparation / purpose | Official product details |
| --- | --- | --- | --- |
| User's VRChat avatar | Yes | Scene-root Descriptor, valid rig, unit-scale/unrotated root | Chosen by user; no fixed avatar purchase |
| User's outfit A | Yes | Already fitted/rigged; explicitly selected groups; supported lilToon materials | Chosen by user |
| User's outfit B | Yes | Same requirements; non-overlapping A/B groups | Chosen by user |
| Angelring Halo / comet_shop (project identifier; official title to verify) | Yes for this halo integration | User purchases/imports it, positions it under the avatar, supplies visual child and original FX_Angelring_Halo controller | **Author to add official URL, exact title, tested download version/date, and creator authorization record** |
| User's AudioClip | No | Optional local forward-only sound; empty audioPath is silent | User supplies a clip they have permission to use |
| Neon wings, sword, other accessories | No | Optional user-selected outfit parts or explicit adapters | No mandated products in core |

The author plans to obtain the halo creator's authorization. This repository currently contains no record that it has been granted. Do not use this draft as evidence of creator approval. Users obtain product files themselves under the applicable product terms; no product files are published here.

The halo binding defaults are Angelring_Halo_White/Halo_Position and Assets/comet_shop/Angelring_Halo/Animation/FX_Angelring_Halo.controller; explicit bindings can use different instance names. The adapter currently targets this product's controller/particle structure, not arbitrary halo products.

See [PLUM_ADAPTER_ASSETS.md](PLUM_ADAPTER_ASSETS.md) for the optional historical Plum reproduction list. Its school uniform, Noir, sword, stockings, wings, and face-tracking assets are **not required for core installation**. Plugin requirements are in [COMPATIBILITY.md](COMPATIBILITY.md).

All copied scan materials, halo mesh derivatives, generated controllers, and validation assets remain in the consuming Unity project. They are excluded from Git and releases.
