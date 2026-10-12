# 实现与本地文件

本包仅含 Editor 程序。运行行为生成到 FX Animator、标准动画曲线、材质和 SDK 音频 StateMachineBehaviour，不依赖上传自定义 MonoBehaviour。

`KitRecipe.settings` 提供绑定、时序、音频与身体遮罩配置；`Presets/plum-verified.json` 是初始值。Create Recipe 创建本地 ScriptableObject，后续执行不会覆盖已有配置。

`KitInstaller` 维护 `Assets/ScanKitLocal/Registry.asset`，记录源角色、安装副本和本包拥有的输出目录。每次生成独立 `Generated_<id>`，先验证，再切换副本；原角色不写入换装组件。当前仅支持一个工程中的一套安装，不允许跨场景盲目覆盖。

`ScanBuilder/ScanMotion` 生成4个换装状态、材质互补裁切和光环曲线。预设 visual duration=3.65 秒；clip 帧量化可能使实际长度略有变化。音效前奏独立于视觉曲线，无声分支不等待前奏。

`ScanBodyMasks` 只迁移预设明确列出的身体裁切规则，防止两套衣服同时启用时永久删除身体。`ScanHaloDefaults` 在本地复制光环原 FX 控制器并移除调节层与参数。

`ScanBuildPreparation` 在 DressingTools 前启用待构建的 A/B 组并移除其冲突的 cabinet-anim 模块；Play Mode 通过已验证的 DressingFramework 生命周期接口提前执行同样处理。`ScanBuildCalibration` 在穿衣完成后重新校准每个渲染器的裁切坐标，静态大剑转为单骨骼蒙皮以保持移动坐标一致。

`BOutfitWingPreparation` 在 MA 绑定结束后、lilToon 扫描之前将翼接入扫描，保持皮肤骨骼与原材质。扫描材质保存为实际 `.mat` 资产，避免编辑器显示正常但上传包材质引用丢失。待机切回商品原材质。

`KitValidation` 检查版本、绑定、形态键、时序、扫描线数值对齐、资产持久化、shader 编译状态；本地构建检查在临时副本按 SDK 注册顺序执行回调，检查处理后的路径与翼曲线。DressingFramework 的 SDK 入口检查调用栈，非 SDK 调用会被忽略，因此本地检查使用该版本的 AvatarBuilder 入口执行穿衣阶段。这是本地预处理模拟，不生成 `.vrca`，不替代真实 SDK 打包、视觉检查或游戏验证，也不自动上传。

换装 Bool 参数为 `KAT/OutfitB`，可选音效 Bool 为 `KAT/Sound`。需为两个同步 Bool 预留参数空间；原商品和头像参数开销需单独计算。
