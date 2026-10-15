# 多文件资源列表

## 用途与界面

`property:multiplefile` 是 SingleListTab 的 Count 操作项模板，不是把整份列表写进一个字符串的编辑器。
顶部提供“添加文件”“清空文件”，随后每项显示一行 source、一行 dest，以及该项的删除按钮。
支持多节点编辑；沿用 SingleListTab，仅在各节点 count 相同时展开条目。
同一索引的字段通过现有 BoundProperty 显示冲突并批量写回，不按文件名匹配条目。
不改变现有 `property:file` 的单文件行为。

## 配置结构

直接使用 SingleListTabTerm<MultipleFilePropertyViewItemListTerm>，不新增专用页签。
将自定义 MultipleFile Count term 配到 Count，将文件 ListTerm 配到 VariableProperty。
Count term 派生 PropertyItemTerm，其 Mapping 仍供 SingleListTab 读取项数；
GetViewModel 返回按钮界面，不显示数字输入框，无需修改 SingleListTab。
不需要额外往 ImmutableProperty 再放一组按钮。

以下对象可放在 .prop 配置的 Tabs 数组中：

```json
{
  "$type": "LuaSTGEditorSharpV2.PropertyView.Configurable.SingleListTabTerm`1[[LuaSTGEditorSharpV2.PropertyView.Specialized.MultipleFile.MultipleFilePropertyViewItemListTerm, LuaSTGEditorSharpV2.PropertyView]], LuaSTGEditorSharpV2.PropertyView",
  "Caption": { "Neutral": "Files", "Localized": { "zh": "文件" } },
  "Count": {
    "$type": "MultipleFile",
    "Mapping": { "Key": "count", "DefaultValue": "0" },
    "Files": {
      "Source": { "Key": "source_{0}" },
      "Destination": { "Key": "destination_{0}" }
    }
  },
  "VariableProperty": {
    "Files": {
      "Count": { "Key": "count", "DefaultValue": "0" },
      "Source": { "Key": "source_{0}" },
      "Destination": { "Key": "destination_{0}" }
    }
  }
}
```

- 页签使用现有泛型 SingleListTab；MultipleFile 短名只用于 Count 项。
- Count.Mapping 供 SingleListTab 读取数量，也覆盖操作项的 Files.Count。
- ListTerm 只按传入的 count 展开，不捕获或比对节点中的 count；非正数不产生条目，与 SmoothSetValue 的循环展开方式一致。
- VariableProperty.Files.Count 仅供条目的删除操作读写数量，不参与列表展开。
- 包作者须让 Count.Mapping、删除操作和资源收集器的计数映射一致，并让各处 Source/Destination 映射一致；不新增跨 term 或跨服务校验。
- Source/Destination 按 NodePropertyCapture.Format(index) 格式化，默认索引从 0 连续排列。
- ListTerm 按传入数量展开 ItemTerm，经现有工厂创建 Bound VM，不解析实际属性名来发现项数。
- Count 与条目 VM 都在 Initialize(PropertySource...) 中保存工厂提供的 token，不需要 SetToken 或 ItemTerm.Token。实际属性页在零选中时不创建这些 VM。
- Count.Editor 默认 multiplefile；VariableProperty.Editor 默认 multiplefileEntry，可通过配置覆盖 Name/Parameters；工厂接收配置后的 Editor，不重新硬编码。
- 默认 count capture 的 DefaultValue 为 0；Count.Mapping 示例中显式给出 0，使缺失数量可按空列表编辑。
- 属性面板只读写有效范围 0..count-1，不扫描或清理范围外的属性。

## 存储与操作

列表存为 count 与每项独立的 source/destination 属性，不用竖线分隔字符串，也没有共享字符串的多 VM 读改写。

添加文件按选择器返回顺序追加到每个选中节点自己的列表末尾，允许重复 source 和 destination，不去重、不覆盖旧条目。
source 保存选择器返回的路径；新 destination 初始化为文件名。
后续修改 source 不联动 destination，修改 destination 也不改变 source。
初始化新项不是构建读取时的兼容回退。

删除保持剩余项顺序，将后续 source/destination 成对前移，删除尾项属性并减少 count。
多选删除作用于各节点的同一索引，保留各节点自身其余条目的值。
清空删除有效范围内现存的配对属性，将 count 设为 0；其他属性不变。
多选清空分别处理各节点自己的有效范围，不要求各节点 count 相同。
空列表清空、取消选择不产生编辑命令。
任一选中节点的 count 无效时，禁用添加/清空，并通过按钮 ToolTip 提示，不自动写成 0。

添加、删除、清空各为一个组合命令，所有选中节点的参数和目标键在执行前准备好，每个节点的 count 在该节点的条目变更之后更新。
结构操作通过 EditResult.ShouldRefreshView 请求现有宿主在整个命令执行完成后刷新。
撤销/重做由既有宿主重建属性页；不能将一个撤销单元误认为属性通知也被批量合并。
编辑单个字段仍使用独立绑定，不要求重建条目。
复用既有多选绑定、列表展开和组合命令，不新增列表合并或文件名对齐规则。

## 列表资源收集

在 .resg 中使用：

```json
{
  "$type": "LuaSTGEditorSharpV2.Core.Building.ResourceGathering.ConfigurableFileListResourceGatheringService, LuaSTGEditorSharpV2.Core.Building",
  "TypeUID": "YourFileListNode",
  "Files": {
    "Count": { "Key": "count", "DefaultValue": "0" },
    "Source": { "Key": "source_{0}" },
    "Destination": { "Key": "destination_{0}" }
  }
}
```

收集器按 count 与索引顺序读取配对，在产出本节点任何资源之前校验有效项。
非法 count、缺失或空的 source/destination 报错，不用 basename 补齐 destination。
重复条目原样保留，目标冲突策略交给后续构建集成处理。
每项分别产出到现有活动资源组；空组栈不自动建立默认组。
处理完本节点列表后沿用现有子节点遍历。

## 范围与验证

此能力不自动提供 AddFile 包资产、sharpconv Archive 展开、默认构建节点或复制目录修复。
无旧 V2 存储迁移或兼容回退；V1 导入须由后续转换器写入 count 与成对字段。

回归应覆盖 SingleListTab 配置/短名反序列化、多选字段冲突、不同长度列表的追加/清空、重复追加、逐项删除、
独立字段、撤销/重做、XML 保存重载、无效数据与收集顺序。
界面还须后台验证窄面板、长路径、多项、空列表、多选冲突、非法 count 的 ToolTip 及结构操作后的刷新。
