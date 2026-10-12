# 可选适配

[English](ADAPTERS.md) | 简体中文

默认 `Presets/core.json` 的 A/B 组、身体遮罩和音频为空，商品服装专用适配均关闭。用户在本地 CoreRecipe 明确填入绑定。光环仍使用外部购买的 Angelring Halo。

| 设置 | 默认 | 功能 |
| --- | --- | --- |
| usePlumAdapter | false | 启用历史 Bag_On/GSToggle 默认值及完整额外依赖版本检查 |
| useDressingToolsAdapter | false | 在副本/构建临时对象准备所选衣装，移除冲突 cabinet-anim，保留绑定与形态模块 |
| useWingAdapter | false | 按 wingName 在 MA 绑定后接入独立翼对象，按实际渲染器数量验证 |
| allowSimpleMaterialConversion | false | 明确开启旧版简化非 lilToon 转换，扫描期间可能丢失原 shader 的效果 |
| bodyPath / bodyMasks | 空 | 单个身体的显式 path、shape、outfitA、outfitB；不改变无关形态键 |
| audioPath | 空 | 本地可选 AudioClip，生成独立开关与正向前奏 |

`Tools/ScanKit/6 Create Optional Plum Recipe` 将 `Presets/plum-verified.json` 载入 `Assets/ScanKitLocal/PlumRecipe.asset`，启用历史适配和绑定。不购买、导入或穿戴任何商品，不改默认 CoreRecipe。通过 Setup 窗口或明确 API 安装该 Recipe。

0.1 的本地 Recipe 保留，不静默迁移；升级时明确核对标志与绑定。旧变身实现保留在自己的场景，不自动迁移。

用户自选兼容翅膀优先作为普通 B 组。只有需要已记录的独立翼接入方式时才启用专门适配；同一渲染器不可同时走两条路线。A/B 共用衣服或配件留在组外。

只有准备好的衣装已使用 DressingTools 时才启用其适配。DressingTools 相关插件及 Poiyomi 不属于强制核心依赖，历史 Plum 预设检查完整基线。工程中已安装的可选插件仍可能执行自身构建回调，即使本包适配关闭；本地验证会说明测试环境，不据此声称已测试仅含最小依赖的新工程。
