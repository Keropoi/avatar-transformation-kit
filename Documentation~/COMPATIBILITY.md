# 准备条件与版本锁定

## 实测基线

| 组件 | 当前工程版本 |
| --- | --- |
| Unity / 平台 | 2022.3.22f1 / Windows PC Avatar |
| VRChat Avatars + Base | 3.10.1 |
| Modular Avatar | 1.16.2 |
| NDMF | 1.11.0 |
| VRCFury | 1.1336.0 |
| lilToon | 2.3.2，含下述窄范围兼容准备 |
| Poiyomi Toon / ThryEditor（原大剑材质） | 10.0.24 / 2.74.3 |
| DressingTools | 2.6.0 |
| DressingFramework | 2.1.1 |
| Chocopoi AvatarLib | 1.1.1 |
| Gesture Manager（本地测试） | 3.9.7 |
| MCP for Unity（Agent 桥接） | 10.3.1-beta.7；commit `142ea0af4890337f0ff6658df61db23982313717` |

版本表记录当前验证环境，不代表这些是最新版本。安装器默认要求核心依赖一致。Quest/Android、任意商品替换、任意体型及其他版本不在第一版保证范围。

## 素材准备

- 角色是保存场景内的根对象，带有效 Descriptor 与原有表情/菜单/衣装配置；不得已经安装旧版变身模块。
- 预设要求角色根节点旋转为零、缩放为一；保留素材已准备好的内部骨骼缩放。
- A/B 衣服已经按商品说明穿戴、绑定和调整。预设中的顶层分组名见 `Presets/plum-verified.json`。
- 光环和 `AngelNeonWing(B)` 已放在角色下、位置正确；翼的 MA Bone Proxy 指向 Chest 并保持世界姿态。不要让 Agent 猜测商品如何穿戴。
- Source halo 的 MA Merge Animator 仍可用；生成器在副本上移除其调节菜单，保留默认粒子。
- 身体 `Body_base` 存在 Shoulder_OFF、Chest_2_OFF、Spine_1_OFF、Ankle_OFF。预设 A=100/B=0；体型其他形态键不强行重设。
- 大剑、背包等原商品开关已经正常。生成器只改动受控衣装在副本上的启用与裁切；不替换原有表情控制器。
- 音频是本地合法可用的可选 AudioClip。当前复现参数 audioLeadSeconds=0.7、audioVolume=0.65。

## lilToon 2.3.2 兼容准备

该基线曾在优化 shader 时把被注释的 include 行当作有效 include，导致 `CBUFFER_START` 编译错误。相关上游现象见 [lilToon issue #366](https://github.com/lilxyzw/lilToon/issues/366)。本工程的窄范围处理是：在 `CustomShaderResources/Misc/ReferenceUVs.lilblock` 中删除三条以 `//#include` 开头、分别指向 `Includes/lil_pipeline_brp.hlsl`、`Includes/lil_common.hlsl`、`Includes/lil_common_appdata.hlsl` 的**注释行**。不要删除实际有效的 include。

这是特定 2.3.2 文件结构的处理方式，不是面向未来版本的通用补丁。操作前保留原文件备份，核对版本与三条注释行；不符合时停止并报告。本包不分发修改后的第三方 shader，也不会自动改写 Packages。用户按本文准备后，Agent 可继续安装。首次安装 Preflight 会检查是否仍有该类注释 include；shader 更新后应重新验证。

## 打包形式

包采用 Unity 包目录结构，生成数据写入 Assets 而非只读包目录；本次提供源码包，尚未提供 VCC 自动安装仓库索引。`vpmDependencies` 记录兼容依赖，导入本地目录不能代替通过 VCC 预先安装这些依赖。格式依据 [VPM 官方文档](https://vcc.docs.vrchat.com/vpm/packages/)。
