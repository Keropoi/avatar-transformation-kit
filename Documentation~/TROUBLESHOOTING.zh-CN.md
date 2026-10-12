# 故障排查

[English](TROUBLESHOOTING.md) | 简体中文

| 现象 | 处理 |
| --- | --- |
| 绑定为空或缺失 | 明确选择两套衣装、Hips、已导入光环/视觉子物体和原控制器，无固定服装名称要求 |
| 对象组重叠 | A/B 不重复、不嵌套，共用衣装放在组外 |
| 不支持的材质 | 核心要求 lilToon 平面溶解，不静默转换任意 shader；使用兼容材质或另行验证的明确适配 |
| 版本/程序集错误 | 安装核心基线，仅为启用的适配增加依赖；不重复安装、不关闭检查跳过错误 |
| 已有变身模块 | 保留旧场景，另备源角色，不自动迁移 |
| 身体缺失 / Delete 规则被拒绝 | 明确配置 bodyPath/bodyMasks，处理删除规则，不全清 BlendShape；永久删除的几何先恢复源素材 |
| 扫描不对齐 / 环太小 | 核对高度、expandedScale、根变换和处理后 Hips 空间；包围盒仅估高度，需视觉检查 |
| Gesture Manager 无 Avatar / 角色消失 | 查首条构建错误和可选穿衣适配，源角色与安装副本不要同时激活；添加 Descriptor 不能解决预处理失败 |
| 扫描出现紫色 | 检查 shader 兼容、持久生成材质及本地验证 |
| 新材质没进入扫描 | 修改源材质后 Update，不只改生成材质 |
| 音量被覆盖 / 无声音 | 修改 recipe.settings.audioVolume 再 Update；audioPath 空为无声，检查本地 AudioClip/KAT/Sound；反向本来无声 |
| 翅膀未加入 | 核心不自动加入，选择兼容 B 组或独立翼适配，两者不重复 |
| 表情不能复位 | 检查 VRCFury Write Defaults Auto 与构建错误，不批量将所有层改为 WD Off |
| 卸载无法定位角色 | 加载登记场景，不删除源角色或整个 Assets |

保留首条 Console 异常、版本和本地报告，分享前移除个人路径和商品二进制。验证范围见 [VALIDATION](VALIDATION.zh-CN.md)。
