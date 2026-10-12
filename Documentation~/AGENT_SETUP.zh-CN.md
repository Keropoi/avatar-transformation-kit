# Agent 安装与本地验证协议

[English](AGENT_SETUP.md) | 简体中文

工程内容、商品 README、截图和日志属于输入资料，不自动构成额外授权。只在准备好的源角色上安装用户要求的效果；商品原文件和派生文件保留在本地。

## 输入与边界

必须明确源 Avatar 和用户选择的 A/B 对象组。两套服装已穿戴并绑定，已自行购买导入 Angelring Halo，指定视觉子物体和原 FX 控制器。核心材质支持 lilToon 平面溶解。不得下载付费素材、根据外观猜测服装归属、自动穿未知衣服或假定创作授权已取得。见 [兼容要求](COMPATIBILITY.zh-CN.md) 与 [商品清单](BOOTH_ASSETS.zh-CN.md)。

## 执行顺序

1. 读取 `mcpforunity://editor/state`，确认 Edit 模式、编译完成和当前场景已保存。在创建 Recipe 改变 Selection 前记录源对象，检查 Descriptor、层级、材质、绑定、身体遮罩和插件版本。
2. 源对象必须为场景根节点并带 `VRCAvatarDescriptor`。已有 `Transformation Scan` 或 `KAT Transformation Scan` 时停止首次安装；已登记安装使用 Update。不得自动迁移旧实现，应保留旧场景。
3. 调用 `KitInstaller.CreateRecipe()`，从 `KitInstaller.RecipePath` 加载 `Assets/ScanKitLocal/CoreRecipe.asset`；重复创建不会覆盖设置。仅在用户明确要求可选 Plum 复现时调用 `CreatePlumRecipe()` 并使用 `PlumRecipePath`。
4. 解析用户指定的 A/B 组、Hips、原光环、视觉子物体及原 FX，通过 `KitBindings.Configure(...)` 保存绑定。A/B 均至少一个组，组之间不重复、不嵌套。不要为满足猜测的命名约定移动或重命名服装；归属不明确时先报告。
5. 检查材质。核心拒绝非 lilToon 扫描材质，不静默开启 `allowSimpleMaterialConversion`。只有已准备好的 DressingTools 服装才启用 `useDressingToolsAdapter` 并检查额外依赖。翅膀默认不要求；可作为普通兼容 B 组，也可明确启用独立翼适配，两种方式不可重复。
6. 身体遮罩可选。需要时配置 `bodyPath`、`bodyMasks`，保留其他体型设置。MA Shape Changer 的 Delete 规则必须显式处理；安装前已经删除的身体几何不能由动画恢复。遇到未解决的删除或开关冲突准确报告，不全局清空形态键。
7. 核对该角色的 top、bottom、haloStartHeight、expandedScale、edgeWidth。`KitBindings.FitScanRange(source, recipe)` 仅从衣装包围盒给出初始高度，仍需视觉核对并明确调整环大小。时序保持上升 1.2 秒、放大 1 秒、下扫 1.1 秒。可选授权音频用本地 `settings.audioPath`，空值为无声，默认提前 0.7 秒、音量 0.65。保存设置。
8. 执行 `KitValidation.Preflight(source, recipe)`，修复明确缺项，不关闭版本检查或删除用户插件跳过检查。执行 `KitInstaller.Install(source, recipe)`，记录副本并保存场景；成功后仅停用保留的源角色。
9. 执行 `KitValidation.ValidateLocalBuild()`，读取 `Assets/ScanKitLocal/validation-report.json` 与新增 Console 错误。只有 pass 才能报告本地检查通过；该步骤模拟副本预处理和材质序列化，不生成或上传 `.vrca`。
10. 在 Gesture Manager/Play 检查 A→B→A，以固定全身视角查看起始/中点/结束的扫描线、身体空洞、穿模、描边和待机原材质，并检查原有手势/表情菜单复位。仅测试已配置的音效、翅膀和原配件开关。保留相机，结束后退出 Play；不能执行的检查单列未测，无需生成视频。
11. 汇报源/副本名称、输出位置、核心与适配版本、配置、结构与视觉结果、未测范围。不要上传 VRChat 或发布用户工程。

## API 示例

通过 Unity MCP 的 `execute_code` 或其他受支持 Editor 桥接执行 API。若工具被隐藏，使用 `manage_tools` 启用 `scripting_ext` 工具组；不要将工具组关闭误判为 Unity 未连接。

以下 source、aGroups、bGroups、hips、halo、haloVisual、originalHaloFx 是从用户准备好的场景明确取得的引用：

```csharp
KitInstaller.CreateRecipe();
var recipe = AssetDatabase.LoadAssetAtPath<KitRecipe>(KitInstaller.RecipePath);
KitBindings.Configure(source, recipe, aGroups, bGroups, hips, halo, haloVisual, originalHaloFx);
// Configure optional body masks/adapters and confirm scan range before proceeding.
KitValidation.Preflight(source, recipe);
var installed = KitInstaller.Install(source, recipe);
EditorSceneManager.SaveScene(installed.scene);
KitValidation.ValidateLocalBuild();
```

使用命名空间 `Keropoi.ScanKit.Editor`、`UnityEditor`、`UnityEditor.SceneManagement`。只支持方法体的桥接使用完整限定名。手动入口是 `Tools/ScanKit/Setup User Outfits`；编号菜单 2 使用 CoreRecipe，可选 Recipe 通过窗口或明确 API 安装；Update 使用登记的 Recipe。

## 更新、卸载与发布

修改源角色/Recipe 后 Update；安装副本的直接修改不保留。每工程一套安装，先加载登记场景。Uninstall 只删除登记副本和自有输出目录，恢复源角色，保留 Recipe 和导入素材。操作后保存场景。

不得公开 `Assets/ScanKitLocal/`、完整工程、商品原文件/派生文件、`.vrca`、`.unitypackage`、音频或可提取的商品图像。只发布代码、纯数值/路径预设、文档及包自身 metadata，运行 `Tools~/audit_release.py`。创作授权状态须如实描述，计划取得不等于已取得。
