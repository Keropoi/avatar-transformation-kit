# Avatar Transformation Scan Kit

[English](README.md) | 简体中文

面向已准备好的 Plum VRChat Avatar 的变身工具包，当前为 `0.1.0-preview.1`。

初步目标：**在已验证的素材组合、插件版本及准备条件下，用户一句话即可让 Agent 完成安装和本地验证。** 本版本提供生成器、Plum 预设、安装/更新/卸载/检查入口及 Agent 文档；不承担自动购买、任意模型自动穿衣或游戏上传。

## 效果

- 光环上升 1.2 秒；上升结束后放大 1 秒，两段均先快后慢。
- 光环用 1.1 秒下降扫描，先慢、再快、再慢；A/B 服装互补裁切。
- B 翅膀在扫描中逐渐显现，待机恢复原材质；大剑保持 B 专属及原有开关。
- 可选音频在正向变身前播放 0.7 秒前奏，音量 0.65；反向不播放，有独立音效开关。未配置音频时仅安装无声变身。
- 身体防穿模 BlendShape 与换装协调；光环商品的调节菜单移除，默认粒子保留。
- 使用 VRCFury 的 Write Defaults Auto 修复；不添加人体动作或镜头动画。

## 准备与安装

1. 对照 [Booth 商品清单草稿](Documentation~/BOOTH_ASSETS.zh-CN.md)，自行购买并导入素材。商品文件、原素材派生材质/网格/控制器、音频均不在仓库内。
2. 按 [准备条件与版本](Documentation~/COMPATIBILITY.zh-CN.md) 配好 Windows Avatar 工程：已穿好衣服、绑定好骨骼、放好光环和翅膀，按预设命名分组。保存场景。
3. 下载此仓库的包，将其内容置于 `Packages/com.keropoi.avatar-transformation/`。先安装依赖，再放入本包。不要在 Assets 和 Packages 重复安装。
4. Unity MCP 连接正常后，向 Agent 发送：

> 请先读取 Packages/com.keropoi.avatar-transformation/Documentation~/AGENT_SETUP.zh-CN.md。素材与插件已按 Plum 预设准备好，请为当前选中的 Avatar 安装变身效果并完成本地验证；如有缺失请准确报告。

Agent 会检查前置条件、生成本地配置和角色副本、执行安装与 SDK 预处理检查，并汇报验证范围。没有 Agent 时也可使用 `Tools/ScanKit` 菜单。

## 更新与卸载

安装在新建角色副本上进行；成功后原角色仅变为不激活。第一版每个工程只管理一套安装。

修改**原角色**的材质、准备状态或本地 Recipe，再执行 `3 Update from Source Avatar`；系统先生成并检查新副本，再替换旧副本。直接修改已安装副本的改动不会在更新时保留。历史生成目录暂留作追溯，卸载时一并清理。

执行 `5 Uninstall` 会删除受本包管理的副本与生成目录，恢复原角色之前的激活状态；保留购买的素材和 Recipe。安装、更新和卸载后保存场景。

## 文档与当前边界

- [Agent 操作流程](Documentation~/AGENT_SETUP.zh-CN.md)
- [Booth 商品清单（请仓库作者补全）](Documentation~/BOOTH_ASSETS.zh-CN.md)
- [版本、准备条件与兼容处理](Documentation~/COMPATIBILITY.zh-CN.md)
- [配置、实现与验证范围](Documentation~/ARCHITECTURE.zh-CN.md)
- [故障排查](Documentation~/TROUBLESHOOTING.zh-CN.md)
- [验证记录](Documentation~/VALIDATION.zh-CN.md)

本地验证不等于上传后游戏验证。不同商品版本、体型、shader 或衣装结构暂不属于已验证支持范围。仓库仅公开代码供查看，**目前未授予开源许可证**，详见 [LICENSE](LICENSE)。
