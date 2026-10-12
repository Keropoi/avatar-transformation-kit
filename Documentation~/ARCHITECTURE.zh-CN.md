# 实现与本地输出

[English](ARCHITECTURE.md) | 简体中文

Editor 代码生成 FX 动画、持久扫描材质、MA 集成和可选 SDK 音频行为，不上传自定义 MonoBehaviour。

- KitRecipe/ScanSetupData 保存明确的本地绑定；core.json 提供时序但 A/B、身体和音频为空，plum-verified.json 是可选复现预设。
- KitBindings.Configure 保存用户选择的对象和导入控制器路径，不移动对象；FitScanRange 估算 top/start/bottom，KitSetupWindow 提供手动操作入口。
- KitInstaller 在副本及独立 Assets/ScanKitLocal/Generated_<id> 目录操作，Registry.asset 记录归属；检查完成再切换，源角色保留并停用。更新从源角色重新生成，卸载只清理自有副本/目录，保留 Recipe 和商品，每工程一套安装。
- ScanBuilder/ScanMotion 生成四个状态及同一扫描平面，视觉总时长 3.65 秒，上升 1.2、放大 1、扫描 1.1 秒。可选音效独立前奏，无声分支不等待。
- 扫描材质在本地持久保存，保留 lilToon 属性及待机原材质。默认拒绝非 lilToon；明确开启的历史简化转换只保留部分基本属性。
- ScanBodyMasks 只处理显式指定的单个身体形态键，在副本迁移匹配 MA 规则并在扫描期间恢复可见身体，不改无关体型，也不重建被删除的源几何。
- ScanHaloDefaults 在本地复制用户原 Angelring 控制器，移除已知调节层、菜单和参数，保留默认粒子。光环网格/材质派生文件仅在本地生成。
- ScanBuildPreparation 在预处理前启用受控衣装，仅启用适配时清理 DressingTools 冲突；存在 DressingFramework 才装 Play 生命周期桥接。处理后校准由曲线方向识别 A/B，使用共同 Hips 空间，适应绑定后路径变化，无服装名称后缀要求。
- BOutfitWingPreparation 可选，在 MA 绑定后、shader 优化前使用本地持久翼材质；核心不强制翼，启用适配后按实际渲染器数量验证。
- KitValidation 检查准备条件、曲线、材质引用、本地预处理及 Prefab 保存重载。存在 DressingFramework 时在其 SDK 回调位置调用该版本 builder，因为其适配拒绝非 SDK 调用栈。这不是 .vrca 构建或上传。

核心使用一个同步 Bool：KAT/OutfitB；配置音频后增加 KAT/Sound。原角色/商品参数另计。
