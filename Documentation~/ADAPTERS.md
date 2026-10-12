# Optional adapters

English | [简体中文](ADAPTERS.zh-CN.md)

The default `Presets/core.json` has empty A/B groups, body masks, and audio, with all product-specific outfit adapters disabled. Populate a local CoreRecipe with explicit bindings. The halo integration is still the external purchased Angelring Halo.

| Setting | Default | Effect |
| --- | --- | --- |
| usePlumAdapter | false | Enables historical Bag_On/GSToggle defaults and the full recorded optional dependency check |
| useDressingToolsAdapter | false | Prepares selected wearable groups and removes competing cabinet-anim modules on temporary/installed copies; preserves rig/shape modules |
| useWingAdapter | false | Adds a separate wing object identified by wingName after MA binding; actual renderer count is validated dynamically |
| allowSimpleMaterialConversion | false | Explicit opt-in to the old simplified non-lilToon conversion; some original shader effects are lost during the scan |
| bodyPath / bodyMasks | empty | Explicit single-body mask rules: path, shape, outfitA, outfitB; unrelated shapes remain intact |
| audioPath | empty | Optional local AudioClip; creates sound switch and forward-only prelude |

`Tools/ScanKit/6 Create Optional Plum Recipe` loads `Presets/plum-verified.json` into `Assets/ScanKitLocal/PlumRecipe.asset`. It enables the historical adapters and bindings. It does not buy, import, or dress any products, and does not change the default CoreRecipe. Install this recipe through the setup window or explicit API.

Existing 0.1 local recipes are preserved, not silently migrated. Review their flags and bindings explicitly when upgrading. An installed legacy transformation needs its own preserved scene; automatic migration is outside scope.

For user-chosen wings with supported materials, prefer selecting their prepared object as an ordinary B group. Use the dedicated adapter only when needed for the recorded separate-wing integration. Never include the same renderers through both routes. Shared clothes/accessories should stay outside the A/B groups.

Use DressingTools only when it is already part of the prepared outfits. Its dependencies and Poiyomi are not mandatory core dependencies. The historical Plum preset checks the full baseline. Optional plugins present in a project may still run their own build callbacks even with the kit's adapter disabled; local validation reports that environment and does not prove a minimal-dependency project has been tested.
