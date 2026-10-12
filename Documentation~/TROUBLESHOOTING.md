# 故障排查

| 现象 | 检查与处理 |
| --- | --- |
| 编译报错 / 找不到程序集 | 先按版本表安装依赖；不要把工程生成目录当作包；检查是否重复安装。 |
| Preflight 拒绝版本 | 使用记录版本，或在单独测试工程验证新版本；不要静默关闭检查并声称已支持。 |
| 已存在 Transformation Scan | 本包不自动迁移旧实现。保留原工程，准备无变身的已穿衣源角色。 |
| Gesture Manager 无 Avatar / Play 模式角色消失 | 查第一条 SDK/DressingTools 错误，确认 Play 生命周期准备已安装，源对象与副本没有同时参与构建。不要靠重新添加 Descriptor 掩盖构建失败。 |
| 扫描中翅膀变紫、待机正常 | 检查生成翼材质是否为持久资产、构建后 PPtr 是否丢失、shader 是否有编译错误；执行本地验证及 shader 兼容检查。 |
| 身体缺失 | 检查 Body_base 四个预设形态键与 MA Shape Changer；不要全局清空体型形态键。 |
| 新材质没有进入扫描 | 修改原角色的原材质，再执行 Update。不要只编辑生成的扫描材质。 |
| 音量修改无效 | 修改 recipe.settings.audioVolume 再 Update；音量也写入了动画，单独改 AudioSource 会被覆盖。 |
| 没有声音 | audioPath 为空即无声版本；检查本地 AudioClip、KAT/Sound、前奏状态，反向本来不触发。 |
| 表情无法复位 | 检查 VRCFury Write Defaults Auto 功能和构建错误；不要把整个 Avatar 所有层直接批量改为 WD Off。 |
| 卸载找不到角色 | 打开 Registry 记录的安装场景，避免手动移动/删掉登记副本。不得因此删除原 Avatar 或整个 Assets。 |

遇到失败保留首条 Console 异常、版本和本地验证报告供排查，分享前移除个人路径及商品二进制文件。
