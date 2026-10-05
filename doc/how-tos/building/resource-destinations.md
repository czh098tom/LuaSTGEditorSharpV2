# 资源目标路径与默认构建

## 三种独立数据

| 数据 | 含义 | 示例 |
| --- | --- | --- |
| source | 源文件位置；由节点映射到 file_path、path 或 source_i | assets/boss.png |
| destination | 归档内文件路径，包含文件名；可带归档根分隔符 | characters/boss.png、/characters/boss.png |
| ResourceGroup | 收集、筛选使用的标签 | Default |

运行时 resource_name / identifier 是第四种独立数据；它不是输出文件名。
组名不自动成为输出目录，也不拼到 destination 前。
生成 Lua 时应使用与资源收集一致的 destination，Lua 字符串转义在生成边界执行。
只配置收集器不会自动修改代码生成器中的文件路径参数。

## 单文件资源声明

以下 .resg 声明使用现有属性键，不要求节点重命名 source：

```json
{
  "$type": "LuaSTGEditorSharpV2.Core.Building.ResourceGathering.ConfigurableResourceGatheringService, LuaSTGEditorSharpV2.Core.Building",
  "TypeUID": "LoadImage",
  "PathCapture": { "Key": "file_path" },
  "TargetNameCapture": { "Key": "destination" }
}
```

NameCapture 不是该服务的配置成员；不要用 resource_name 替代 TargetNameCapture。
声明 PathCapture 后，空白或缺失的 source / destination 会报错。
不再从源文件名回退 destination；导入器和新节点初始化必须显式提供目标。
未声明 PathCapture 的服务仍可作为透明节点遍历子节点。

## AddFile 列表

AddFile 保持一个节点；所有值仍为字符串：

```text
count = 2
source_0 = assets/first.png
destination_0 = images/first.png
source_1 = assets/second.png
destination_1 = images/second.png
```

其 .resg 使用 ConfigurableFileListResourceGatheringService：

```json
{
  "$type": "LuaSTGEditorSharpV2.Core.Building.ResourceGathering.ConfigurableFileListResourceGatheringService, LuaSTGEditorSharpV2.Core.Building",
  "TypeUID": "AddFile",
  "Files": {
    "Count": { "Key": "count", "DefaultValue": "0" },
    "Source": { "Key": "source_{0}" },
    "Destination": { "Key": "destination_{0}" }
  }
}
```

count 是数量唯一依据，索引从 0 开始；范围外属性不参与收集。
有效范围内的条目先全部校验，再按原顺序产出；重复项保留。
非法 count、缺失或空白配对字段报错。
属性页 Count、条目删除以及收集器的映射应一致，详见 [MultipleFile](<../property view/multiplefile.md>)。

## 资源组

未设置初始组、也没有 UseResourceGroup 时，收集器不产出资源。
UseResourceGroup 在遍历子节点期间入栈，完成后出栈。
嵌套资源组使同一资源分别属于多个活动组；不是仅取最近祖先。
UseResourceGroup.resg 的服务为 ConfigurableResourceGroupService，GroupCapture 是属性键字符串。

CaptureResourceGroupTask.InitialResourceGroup 默认 null，表示不推入初始组。
显式空字符串表示一个有效的空标签组，和 null 不同。
GetResourcesToPack 的四参重载保留 GetContextOfNode 的设置语义，再推入初始组。

## DefaultLuaSTGBuilding 的选择项

| 节点属性 | 缺省行为 | 显式值 |
| --- | --- | --- |
| initial_resource_group | 不添加初始组 | 标签字符串；空字符串也是有效标签 |
| keep_first_resource_target | false，后项覆盖前项 | true，保留首次成功复制的源 |

工厂通过 InitialResourceGroupCapture、KeepFirstResourceTargetCapture 读取这些属性。
前者只有节点实际存储对应属性时才启用；后者只接受可解析为布尔值的字符串。
sharpconv 注入的构建节点明确写入 Default 和 true，不改变其他未配置构建节点的默认策略。
该策略只设置在资源 CopyTask 上；生成 Lua 和 root.lua 的复制使用原有默认值。
输出基目录仍来自 build.target_dir，模组输出名称仍来自 output_name。

## 路径与复制行为

CaptureResourceGroupTask 按每个被收集文档自身的目录解析相对 source。
绝对 source 原样定位；解析不依赖启动 CLI 时的工作目录。
导入到另一目录时，sharpconv 负责先把 V1 source 重定位到输出文档的基准。
destination 不受该 source 重定位影响，仍表示归档目标。

DefaultLuaSTGBuilding 在资源复制前用 ParseContextVariableTask 映射收集到的归档目标。
前导 / 或 \ 不表示要写入操作系统根目录：/foo.lua → 输出根/foo.lua，
/x/foo.lua → 输出根/x/foo.lua；普通相对目标保持。
节点属性、GroupedResource.TargetName 和生成 Lua 的引用保留原始归档路径，
不把它们改成文件系统路径；不新增节点字段或配置开关。
原生 Windows 上，根路径映射沿用 V1 quick ZIP 的 SharpZipLib DropPathRoot 规则：
通过 Path.GetPathRoot 识别前 258 字符的根长度，再去掉剩余前导分隔符；归档 key 不截断。
重复前导分隔符不一律拒绝，也不简单全部 TrimStart：
///x/foo.lua → x/foo.lua，//server/share/foo.lua → foo.lua，//x/foo.lua → 空目标并拒绝。
这里只复刻 ZIP 条目名映射，不访问 UNC 网络共享；不能据此宣称其他平台/打包后端或引擎查找一致。
映射前拒绝原 key 的控制字符、盘符及非法字符，防止它们在去根时被隐藏。
映射后的空目标、非法名称及越界仍由 CopyTask 拒绝。
真实包含检查使用当前输出根与规范化后的完整目标路径，不构造 probe 文件名。
重复目标仍按 CopyTask 的规范化落盘路径判定，保留既有首次/覆盖策略。
这项映射只属于 DefaultLuaSTGBuilding 的资源复制，不影响普通 CopyTask、生成 Lua 或 root.lua 的复制。
含内部 .. 的目标沿用现有落盘规范化；V1 ZIP 可保留 .. 条目，
其引擎查找是否等价需独立运行时证据，不能仅凭构建成功认定。

CopyTask 创建每个目标文件的父目录，支持多层相对目标。
空目标、绝对目标、盘符或 UNC 目标、非法名称和逃逸输出根的路径会报错。
源列表和目标列表长度不一致会报错，不静默截断。
同一次 CopyTask.Execute 内按规范化绝对目标识别冲突；Windows 下不区分大小写。
冲突日志记录两个源及保留或覆盖策略；它不等于文件内容去重。
I/O 复制最多尝试五次，最后一次仍失败时记录错误并抛出。
取消令牌可以中断复制和重试等待。

## 验收范围

至少检查真实包发现、无组/空标签/嵌套组、列表顺序、相对 source、嵌套目标、重复目标和失败诊断。
核对输出相对路径及文件字节；同时检查 Lua 中的引用是否指向相同目标。
CLI 日志和非空产物是验收依据，单独的退出码 0 不足以证明成功。
收集器或属性页测试不替代 sharpconv → build 的完整验收。
