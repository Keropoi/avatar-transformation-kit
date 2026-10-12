# Avatar Transformation Scan Kit

[English](README.md) | 简体中文

为已准备好的 VRChat Avatar 生成光环扫描及 A/B 互补裁切换装的 Editor 工具包，当前预览版 `0.2.0-preview.1`。

**用户自行选择并准备 A/B 服装，工具包提供绑定工具和动画生成代码。** 初步目标是在文档中的准备条件满足后，用户一句话即可让 Agent 安装并完成本地验证。服装穿戴、任意 shader 转换及上传 VRChat 不属于安装流程。

## 核心效果

- 光环上升 1.2 秒，随后放大 1 秒，两段均先快后慢。
- 下扫 1.1 秒，明显先慢、再快、再慢；A 在扫描线下方消失，B 在同一扫描线上方出现。
- 待机恢复服装原材质，保留源角色，支持更新和范围受控的卸载。
- 无固定服装命名要求，不强制 Plum 身体、大剑或霓虹翅膀。核心扫描材质必须支持 lilToon 的平面溶解。
- 身体防穿模遮罩和正向音效是可选本地配置。音效默认提前 0.7 秒、音量 0.65；仅配置 AudioClip 后生成独立音效开关。
- 在安装副本使用 VRCFury Write Defaults Auto。不生成举手、姿势或镜头动画。

## 外部光环：用户自行购买导入

本版使用 **Angelring Halo / comet_shop** 商品适配。用户自行购买、导入并将光环放在 Avatar 下，指定原光环 FX 控制器。仓库不分发光环的网格、贴图、材质、动画、粒子或商品派生文件；相关生成操作只在用户自己的工程内执行。

仓库作者计划取得商品原作者的创作授权，**目前仓库尚未记录授权完成**。本预览版不声明已经获得授权。官方商品链接及实测下载版本仍待作者补充，见 [商品清单](Documentation~/BOOTH_ASSETS.zh-CN.md)。

## 通过 Agent 安装

1. 按 [兼容与准备要求](Documentation~/COMPATIBILITY.zh-CN.md) 安装核心插件，提前穿戴并绑定两套服装，导入和摆好自己购买的光环，保存场景。
2. 包内容放在 `Packages/com.keropoi.avatar-transformation/`，不要同时在 Assets 再安装一份。仅在使用可选适配时安装对应额外依赖。
3. 选中准备好的源角色，连接 Unity MCP，将实际 A/B 对象名告诉 Agent：

> 请先读取 Packages/com.keropoi.avatar-transformation/Documentation~/AGENT_SETUP.zh-CN.md。我的 Avatar 和已购买的 Angelring Halo 已准备好，A 服装是【对象名】，B 服装是【对象名】。请安装光环扫描变身效果并完成本地验证，准确报告缺少的条件和未测项。

将括号内容替换为实际场景对象名。Agent 创建本地核心 Recipe，绑定对象，检查材质和身体遮罩，在副本上安装并汇报结果；不得猜测服装的 A/B 归属。

手动安装使用 `Tools/ScanKit/Setup User Outfits`：指定源角色、Recipe、Hips、A/B 组、原光环、视觉子物体和原 FX，保存绑定，按需配置身体遮罩，检查扫描高度和大小，再执行 Preflight、安装和验证。

## 核心工具 + 可选适配

| 功能 | 核心默认 | 可选配置 |
| --- | --- | --- |
| A/B 服装 | 用户指定互不重叠的对象组，使用 lilToon | Plum 初版复现预设 |
| 光环 | 外部购买的 Angelring Halo | 支持的商品适配，仓库不附带素材 |
| 身体遮罩 | 空；不要求 Plum 专用形态键 | 显式身体路径和 A/B 数值；必须处理破坏性删除规则 |
| 霓虹翅膀 | 不要求 | 作为普通兼容 B 组，或启用专门翅膀适配；两种方式不重复使用 |
| DressingTools | 适配关闭 | 对已准备好的 DressingTools 服装启用，并安装记录的依赖 |
| 非 lilToon 材质 | 拒绝 | Plum 复现可明确启用简化转换，不承诺任意 shader 兼容 |
| 音效 | 未配置则无声 | 本地授权 AudioClip，正向播放及独立开关 |

配置见 [可选适配](Documentation~/ADAPTERS.zh-CN.md)。[Plum 商品列表](Documentation~/PLUM_ADAPTER_ASSETS.zh-CN.md) 仅描述历史组合，不是核心版必购清单。

## 更新与卸载

修改**源角色**或本地 Recipe，再执行 `Tools/ScanKit/3 Update from Source Avatar`。系统先生成并检查新副本，再替换旧副本；直接修改安装副本的内容不会在更新时保留。历史输出目录留到卸载时清理。

`Tools/ScanKit/5 Uninstall` 删除登记副本和自有生成目录，恢复源角色之前的激活状态，保留购买素材和 Recipe。每次操作后保存场景。当前每个工程管理一套安装。

## 文档与验证

- [Agent 操作流程](Documentation~/AGENT_SETUP.zh-CN.md)
- [光环与用户素材清单](Documentation~/BOOTH_ASSETS.zh-CN.md)
- [准备与兼容](Documentation~/COMPATIBILITY.zh-CN.md)
- [可选适配](Documentation~/ADAPTERS.zh-CN.md)
- [实现说明](Documentation~/ARCHITECTURE.zh-CN.md)
- [故障排查](Documentation~/TROUBLESHOOTING.zh-CN.md)
- [0.2 验证记录和边界](Documentation~/VALIDATION.zh-CN.md)

代码已经解除原服装组合的强制要求，验证范围仍限于记录的本地测试组合和版本，不等于所有 Avatar、shader 和服装均兼容。本地预处理不等于 SDK 打包或游戏验收。

代码公开供查看，**尚未授予开源许可证**，见 [LICENSE](LICENSE)。商品许可和今后的创作授权与代码授权分别说明。
