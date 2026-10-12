# 准备与兼容要求

[English](COMPATIBILITY.md) | 简体中文

## 记录的基线

以下为测试工程版本，不代表最新版本，默认保持严格版本检查。

| 核心组件 | 记录版本 |
| --- | --- |
| Unity / 平台 | 2022.3.22f1 / Windows PC Avatar |
| VRChat Avatars + Base | 3.10.1 |
| Modular Avatar | 1.16.2 |
| NDMF | 1.11.0 |
| VRCFury | 1.1336.0 |
| lilToon | 2.3.2，含下述准备 |

| 可选适配 / 测试组件 | 记录版本 |
| --- | --- |
| DressingTools / DressingFramework / Chocopoi AvatarLib | 2.6.0 / 2.1.1 / 1.1.1 |
| Poiyomi Toon / ThryEditor（历史大剑） | 10.0.24 / 2.74.3 |
| Gesture Manager（本地测试） | 3.9.7 |
| MCP for Unity（Agent 桥接） | 10.3.1-beta.7；commit `142ea0af4890337f0ff6658df61db23982313717` |

仅启用 DressingTools 适配时要求其版本；可选 Plum 预设检查包含 Poiyomi 的完整历史基线。核心包依赖不再包含这些可选插件。当前测试工程已安装可选插件，尚未测试只安装最小依赖的空工程，也未测试 Android/Quest。

## 输入准备

- 已保存场景中的根 VRCAvatarDescriptor，原表情和绑定有效，角色根旋转为零、缩放为一，不含已有变身模块。
- 用户选择并已穿戴/绑定的 A/B 对象组，两组均非空，组之间不重复、不嵌套。无指定商品或名称要求，共用衣装放在组外。
- 启用的衣装网格渲染器使用支持平面溶解的 lilToon 材质。其他 shader 前置检查拒绝；简化转换必须明确启用，不代表通用兼容。
- 穿戴处理后衣装蒙皮必须绑定至角色 Hips 或其后代；校准使用 Hips 作为共同扫描空间，不替换皮肤骨骼。
- 用户购买的 Angelring Halo 已摆在角色下，保留原 MA Merge Animator、视觉子物体和原 FX。适配在副本移除调节菜单并保留默认粒子，仍针对该商品的层/粒子结构。
- 核对扫描高度和 expandedScale。FitScanRange 从衣装包围盒估算高度，不自动调整光环半径或替代视觉检查。
- 不需要身体遮罩协调时可留空；需要时指定 bodyPath 和 BlendShape 规则。所选服装 MA Shape Changer 的 Delete 必须显式映射。无法重建已经永久删除的网格。
- 原配件和表情开关提前可用。竞争的衣装开关可能需要明确适配，不承诺自动解决任意控制器冲突。
- 可选本地授权 AudioClip，audioPath 空则无音效。可选翼和适配见 [ADAPTERS](ADAPTERS.zh-CN.md)。

## lilToon 2.3.2 兼容准备

该基线曾在优化 shader 时把被注释的 include 行当作有效 include，导致 `CBUFFER_START` 编译错误。相关上游现象见 [lilToon issue #366](https://github.com/lilxyzw/lilToon/issues/366)。本工程的窄范围处理是：在 `CustomShaderResources/Misc/ReferenceUVs.lilblock` 中删除三条以 `//#include` 开头、分别指向 `Includes/lil_pipeline_brp.hlsl`、`Includes/lil_common.hlsl`、`Includes/lil_common_appdata.hlsl` 的**注释行**。不要删除实际有效的 include。

这是特定 2.3.2 文件结构的处理方式，不是面向未来版本的通用补丁。操作前保留原文件备份，核对版本与三条注释行；不符合时停止并报告。本包不分发修改后的第三方 shader，也不会自动改写 Packages。用户按本文准备后，Agent 可继续安装。首次安装 Preflight 会检查是否仍有该类注释 include；shader 更新后应重新验证。

## 发布形式

源码内容放在 `Packages/com.keropoi.avatar-transformation/`，生成文件写到 Assets；依赖通过 VCC 或各自支持的流程独立安装。尚未提供 VCC 仓库索引，格式依据 [VPM 官方文档](https://vcc.docs.vrchat.com/vpm/packages/)。外部光环创作授权尚待记录，仓库不分发商品素材。
