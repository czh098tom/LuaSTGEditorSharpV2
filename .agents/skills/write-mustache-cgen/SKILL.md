---
name: write-mustache-cgen
description: 为 LuaSTGSharpEditor 项目编写、拆分和审查 Mustache 代码生成模板，处理 .cgen 的片段拼接、显式换行和捕获；界面显示模板另按其消费方处理。
---

# Mustache 代码模板

先从配置的 `$type` 确认实际消费方和字段类型，避免把普通字符串、旧 Selection 配置或显示文本误当成代码模板数组。

- 代码模板数组是按顺序直接拼接的片段。JSON 中分成多项不增加 Lua 换行；输出需要换行时在字符串内写 `\n`，需要空格时保留空格。
- 多行模板和过长的单行都可以拆。优先在语句、分支或参数边界拆分，保留 Mustache 标签、必要空白和条件段的范围；不要为了 JSON 排版给输出增加换行。
- 迁移原先依赖隐式换行的数组时，把旧分隔符明确写进片段。检查迁移前后传入渲染器的完整模板文本，而不是只检查数组项数或排版。
- `self`、`context`、`condition` 来自各自捕获配置；`args` 是具体服务提供的数据，不能假定普通 Mustache 生成器都有。`{0:IND}` 只在模板文本阶段展开，不对捕获值中的相同文本再次替换。
- 修改字段拼接约定时盘点该消费方的全部配置。Mustache 显示配置的 Text 数组仍有自身的换行语义，不随 .cgen 一起改变。
- 验证实际生成文本与已有明确预期或真实工程输出。递归调用后的 Tail、条件段、显式换行和缩进须按实际改动范围核对；不靠 Trim 掩盖差异，不按节点名称批量编造行为边界。

片段示例：下面三个数组项输出同一条 Lua 语句，最后的 `\n` 才是输出换行。

```json
"Head": [
  "local value = ",
  "{{{self.value}}}",
  "\n"
]
```

相关实现按需读取：

- 普通生成器：`src/LuaSTGEditorSharpV2.Core.CodeGenerator/Configurable/Mustache/MustacheCodeGeneration.cs`。
- 公共渲染和缩进：`src/LuaSTGEditorSharpV2.Core.CodeGenerator/Configurable/Mustache/CodeFragmentTemplate.cs`、`src/LuaSTGEditorSharpV2.Core.CodeGenerator/CodeGenerationContext.cs`。
- JoinedList 的专用模型：`src/LuaSTGEditorSharpV2.Core.CodeGenerator/Configurable/Mustache/JoinedListCodeGeneration.cs`。
