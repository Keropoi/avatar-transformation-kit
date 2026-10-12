# Draft Booth asset list

English | [简体中文](BOOTH_ASSETS.zh-CN.md)

This draft is based on names and uses in the current project. **It is not a verified shopping list.** Names may refer to folders, Prefabs, or user-renamed objects. The publisher must add official product titles, creators/shops, product pages, and download versions/dates. Entries that cannot be uniquely identified remain pending. No product files are included in this repository.

| Project identifier / provisional name | Purpose | Preset binding | Product link | Version / author verification |
| --- | --- | --- | --- | --- |
| Plum base avatar | Avatar, body mesh, skeleton, and body BlendShapes | Root Descriptor; Armature/Hips; Body_base | Author to fill in | Pending |
| Plum School Uniform | Outfit A uniform, socks, bag, etc. | Plum_SchoolUniform(A) | Author to fill in | Pending |
| A shoes and accessories (source unconfirmed) | Outfit A shoes, neck accessories, etc. | Shoes&ACC(A) | Author to fill in | Pending |
| Noir / C.QuinStore (name to verify) | Outfit B top, arm covers, skirt, etc. | Noir_Plum_BK(B) | Author to fill in | Pending; arm covers and skirt currently use Noir_UV2_WH |
| B shoes (source unconfirmed) | Outfit B shoes | Shoes(B) | Author to fill in | Pending |
| Great sword (source unconfirmed) | Outfit B sword and its original toggles | Great sword (B) | Author to fill in | Pending; preserves Great_SwordOn and other original controls |
| Hosanna_Ccl (product title to verify) | Part of outfit B | Hosanna_Ccl(B) | Author to fill in | Author to confirm the specific submesh uses |
| Stocking (source unconfirmed) | Outfit B stockings | Stocking(B) | Author to fill in | Mesh name includes Lime&Chiffon&Chocolat; insufficient to uniquely identify a product |
| Angelring Halo / comet_shop | Original halo, scan-ring visuals, default particles, and continuous animations | Angelring_Halo_White/Halo_Position | Author to fill in | Controller: FX_Angelring_Halo; version pending |
| Neon Angel Set / harusamezakka | Outfit B neon angel wings | AngelNeonWing(B); wing L / wing R | Author to fill in | Original neon white material; version pending |
| Transformation audio (source unconfirmed) | Optional forward-transformation sound | User supplies recipe.settings.audioPath | Author to fill in | Not distributed; empty configuration produces a silent transformation |
| Plum Face Tracking / Triturbo (optional) | Face tracking already present on the avatar | Prepared avatar's own configuration | Author to fill in | Not a feature supplied by the transformation generator |

Plugin versions are listed separately in [COMPATIBILITY.md](COMPATIBILITY.md). Keep software dependencies separate from required product purchases.

For each entry, add the official title, shop name, official URL, tested version or download date, whether it is required, import order, installation location, material/body modifications, and a link to its license terms. For products supporting multiple avatars, specify the Plum version tested with this preset.

Material copying, scan-mesh baking, and trimming original product controllers all happen inside the user's Unity project. Locally generated files may still contain product content; generation does not make them freely redistributable.
