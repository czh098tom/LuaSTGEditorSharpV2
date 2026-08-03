# LuaSTG Editor Sharp V2 — 系统架构调研报告

> 调研对象：`src/` 下 45+ 个 csproj。本报告基于实际源码与 `test/*.lstgjson` 样例交叉验证。

---

## 一、系统架构图谱

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Application Layer (应用层)                          │
│  ┌──────────────────────────────┐   ┌──────────────────────────────────┐   │
│  │ LuaSTGEditorSharpV2 (WPF)    │   │ LuaSTGEditorSharpV2.CLI          │   │
│  │  App.xaml.cs → HostBuilder   │   │  Program.cs → CLIPluginProvider  │   │
│  └──────────────────────────────┘   └──────────────────────────────────┘   │
└──────────┬───────────────────────────────┬──────────────────────────────────┘
           │                               │
           ▼                               ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│              UI / 服务聚合层 (Service Aggregation, 主机内)                  │
│  View · ViewModel · PropertyView · Toolbox · NodeProfile · NodeProfile.WPF  │
│  DockingWindows · Dialog · UICustomization · WPF · Execution · Debugging    │
│  ResourceDictionaryService · Resources.Shared · CLI.Plugin                  │
└──────────┬──────────────────────────────────────────────────────────────────┘
           │ 通过 [PackedServiceProvider] 反射注册 + DI 容器
           ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                      ServiceBridge 桥接层 (设置适配)                        │
│  ServiceBridge.Building · CodeGenerator · Execution · UICustomization       │
│  (SettingsDisplayDescriptor 把 Core 服务设置 → WPF Settings ViewModel)      │
└──────────┬──────────────────────────────────────────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                    Core Layer (核心层，无 UI 依赖)                          │
│  ┌───────────────┐  ┌─────────────────┐  ┌──────────────────────────────┐   │
│  │ Core          │  │ Core.Command    │  │ Core.CodeGenerator           │   │
│  │ (NodeData,    │  │ (CommandBase,   │  │ (CodeGenerationContext,      │   │
│  │  PackedService│  │  CommandBuffer, │  │  Mustache/NCalc)             │   │
│  │  DI/Json/Host)│  │  CompositeCmd)  │  │                              │   │
│  └───────────────┘  └─────────────────┘  └──────────────────────────────┘   │
│  ┌──────────────────────┐  ┌────────────────────────────────────────────┐  │
│  │ Core.Building        │  │ Core.Analyzer.StructuralValidation         │  │
│  │ (IBuildingTask,      │  │ (ConfigurableStructuralValidation,         │  │
│  │  BuildTaskFactory)   │  │  CanInsert / CanDeactivate)                │  │
│  └──────────────────────┘  └────────────────────────────────────────────┘  │
└──────────┬──────────────────────────────────────────────────────────────────┘
           │ 包加载契约：./package/<name>/*/*.<shortName> (json) + 程序集扫描
           ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│           Package Layer (插件层, 三个内置包, 互相独立)                       │
│  ┌────────────────────┐  ┌────────────────────┐  ┌──────────────────────┐  │
│  │ LegacyNode (9 csproj)│ │ Lua (7 csproj)    │  │ LuaSTGSub (3 csproj) │  │
│  │ Boss/Bullet/Stage..│  │ Audio/Graphics/   │  │ Execution config for │  │
│  │ +SharpProjectConv. │  │ Building/Lua base │  │ LuaSTG sub-projects  │  │
│  └────────────────────┘  └────────────────────┘  └──────────────────────┘  │
│  每个包按 Building/CodeGenerator/PropertyView/Toolbox/ViewModel/Resources 子模块切分 │
└─────────────────────────────────────────────────────────────────────────────┘
```

**关键事实**：三层严格遵循"上层依赖下层，Core 层不反向依赖"原则；Package 之间**互不依赖**，全部只引用 Core；UI 不直接依赖 Package（运行时通过 `PackedService` 反射注册发现）。

---

## 二、核心模块职责

### 2.1 Core 层（基座）

| 模块 | 职责 | 关键类型 |
|---|---|---|
| `LuaSTGEditorSharpV2.Core` | 节点模型、PackedService 抽象、DI、Json 序列化、应用引导 | `NodeData`、`PackedDataProviderServiceBase<>`、`NodePackageProvider`、`ApplicationHostBuilder`、`InjectAttribute`、`ServiceProviderContractResolver` |
| `Core.Command` | 撤销/重做的命令栈、原子/复合命令 | `CommandBase`、`CommandBuffer`、`CompositeCommand`、`CheckedCommand` |
| `Core.Building` | 构建任务流水线、上下文变量、资源收集 | `IBuildingTask`、`BuildingContext`、`CompositeBuildingTask`、`BuildTaskFactoryServiceProvider` |
| `Core.CodeGenerator` | 代码生成器、Mustache + NCalc 模板 | `CodeGeneratorServiceBase`、`CodeGenerationContext`、`CodeData`、`MustacheCodeGeneration` |
| `Core.Analyzer.StructuralValidation` | 节点树结构合法性校验（父子约束、唯一性） | `StructuralValidationServiceProvider`、`ConfigurableStructuralValidation` |

### 2.2 UI/服务聚合层

| 模块 | 职责 |
|---|---|
| `View` + `WPF` | 主窗口、Ribbon、停靠面板宿主、`MainWindow.xaml.cs` code-behind |
| `ViewModel` | `MainViewModel`、`WorkSpaceViewModel`、`DocumentViewModel`、`NodeViewModel`；**`ConfigurableViewModelProvider`** 驱动节点显示文本 |
| `PropertyView` | 节点属性编辑面板，按 TypeUID 动态解析 DataTemplate |
| `Toolbox` | 节点工具箱树，拖拽生成新节点（触发 `InsertCommand`） |
| `NodeProfile` / `NodeProfile.WPF` | 节点档案（预设模板）元数据与 WPF 展示 |
| `DockingWindows` | Docking 面板注册、Ribbon 按钮与分组注册 |
| `Dialog` | 设置/向导对话框宿主 |
| `UICustomization` | 主题资源字典动态切换 |
| `Execution` | `ExecutionTaskFactory` 委托，启动外部 LuaSTG 进程 |
| `Debugging` | 调试输出面板（`IOutputLogWriter`） |
| `ResourceDictionaryService` | pack-URI 资源字典的注册中心 |
| `Resources.Shared` | 公共图标、画笔、DataTemplate |
| `CLI.Plugin` | CLI 子命令注册中心 |

### 2.3 ServiceBridge 层

**唯一职责**：把 Core 的服务设置（如 `CodeGenerationServiceSettings`、`ExecutionConfigServiceSettings`）适配为 WPF 可绑定的 `*SettingsViewModel`，并通过 `SettingsDisplayDescriptorProvider` 暴露到 Settings 对话框。**不含业务逻辑**。

### 2.4 Package 层

| 包 | 节点域 | 特有子模块 |
|---|---|---|
| **LegacyNode** | Boss / Enemy / Bullet / Laser / Object / Stage / Task / Audio / Graphics / Curve / Render | `SharpProjectConverter`（旧工程迁移）、`CLIPluginProvider`（`sharpconv` 命令）、`BuildTaskFactory` |
| **Lua** | 纯 Lua 语言的 Audio / Building / Graphics / Lua 基础节点 | `LanguageBase` 实现（`LuaLanguage`） |
| **LuaSTGSub** | LuaSTG 子项目执行配置 | `LuaSTGSubExecutionConfigService` |

---

## 三、关键接口契约（已源码验证）

### 3.1 节点数据模型（`src/LuaSTGEditorSharpV2.Core/Model/NodeData.cs:13`）

```csharp
public class NodeData {
    string TypeUID;                              // 类型唯一标识，跨包命名空间约定
    bool IsActive;                               // false 则被逻辑遍历过滤
    Dictionary<string,string> Properties;        // 属性全为 string，强类型由 Property<T> 包装
    IReadOnlyList<NodeData> PhysicalChildren;    // 物理子节点（含被 IsActive 过滤的）
    NodeData? PhysicalParent;                    // [OnDeserialized] 回填
}
```
**契约约束**：`Properties` 全为字符串；逻辑子节点 = `PhysicalChildren.Where(n => n.IsActive)`；`PhysicalParent` 仅在反序列化/插入时由父节点回填。

### 3.2 PackedService 双轨加载（`IPackedDataProviderService.cs:9` + `IServiceInstanceProvider.cs`）

```csharp
public interface IPackedDataProviderService<TData> {
    IDisposable Register(string id, PackageInfo packageInfo, TData data);
    PackageInfo  GetPackageInfo(TData data);
}
public interface IServiceInstanceProvider<TData> {
    IReadOnlyCollection<TData> GetServiceInstances(IServiceProvider sp);
}
```
**两种实例来源**（择一）：
1. **JSON 文件扫描**：`./package/<pkg>/*/*.<serviceShortName>` → 反序列化为 `TData`，再 `Register()`
2. **代码动态生成**：实现 `IServiceInstanceProvider<TData>`，零参构造 + 反射发现

`PackedDataProviderServiceBase<>` 内部用 `PriorityQueue<TData, PackageInfo>` 按 `PackageInfo` 优先级解析冲突（高优先级包覆盖低优先级）。

### 3.3 服务短名映射表（**已 grep 全仓库核实**，是最关键的工程契约）

| ServiceShortName | ServiceName | Provider 类 | 所在模块 |
|---|---|---|---|
| `vm` | ViewModel | `ViewModelProviderServiceProvider` | ViewModel |
| `cgen` | CodeGenerator | `CodeGeneratorServiceProvider` | Core.CodeGenerator |
| `build` | BuildTaskFactory | `BuildTaskFactoryServiceProvider` | Core.Building |
| `resg` | ResourceGathering | `ResourceGatheringServiceProvider` | Core.Building |
| `default` | DefaultValue | `DefaultValueServiceProvider` | Core |
| `valid` | Analyzer.StructuralValidation | `StructuralValidationServiceProvider` | Core.Analyzer.StructuralValidation |
| `prop` | PropertyView | `PropertyViewServiceProvider` | PropertyView |
| `cboptions` | — | `ComboboxOptionsServiceProvider` | PropertyView |
| `tool` | Toolbox | `ToolboxProviderService` | Toolbox |
| `profile` | NodeProfileProvider | `NodeProfileProvider` | NodeProfile |
| `execfg` | Execution | `ExecutionConfigServiceProvider` | Execution |
| `resource` | — | `ResourceDictionaryRegistrationService` | ResourceDictionaryService |
| `sharpconv` | — | `SharpNodeConverterServiceProvider` | Package.LegacyNode.SharpProjectConverter |
| (无短名) | CLIPluginProvider | `CLIPluginProviderService` | CLI.Plugin（仅 `IServiceInstanceProvider`，不走文件扫描） |

### 3.4 命令系统契约（`CommandBase` / `CommandBuffer`）

```csharp
abstract class CommandBase {
    bool Executed { get; }
    void Execute(EditorDocument);   // 幂等：已执行会抛 InvalidOperationException
    void Revert(EditorDocument);    // 必须先 Execute
    protected abstract void DoExecute(EditorDocument);
    protected abstract void RevertExecution(EditorDocument);
}
class CommandBuffer(EditorDocument) {  // 不 sealed
    bool CanUndo, CanRedo, IsModified;
    void Execute(CommandBase); void Undo(); void Redo(); void Save();
}
class CompositeCommand : CommandBase {  // 不 sealed
    // 只 catch CommandExecutionException；其他异常不触发回滚
    // 失败时逆序 Revert 已执行的 _innerCommands
}
static class Commands {  // 工厂
    CommandBase? FromFilteredList(IReadOnlyList<CommandBase>?);  // null/empty→null，single→原对象，multi→Composite
}
```
**契约**：
- `Execute()` 必须幂等可逆；`Revert()` 必须先 `Execute()`，否则抛 `InvalidOperationException`
- 复合命令回滚**仅**对 `CommandExecutionException` 生效——自定义异常类型不会被回滚路径捕获，业务命令失败必须包装为 `CommandExecutionException`
- `CheckedCommand`（`src/LuaSTGEditorSharpV2.Core.Command/CheckedCommand.cs`）当前是静态工具类，**未接** `StructuralValidationServiceProvider`（与 Architecture 早期描述不符）；C15 约束要求的"结构性变更必须经校验"目前由调用方自行保障

### 3.5 节点服务契约（`NodeServiceProvider<TService>` + `ContextualNodeServiceProvider<,,>`）

```csharp
abstract class NodeServiceProvider<TService> { TService GetServiceOfNode(NodeData); }
abstract class ContextualNodeServiceProvider<TService, TContext, TSettings>
    : NodeServiceProvider<TService> {
    TContext GetContextOfNode(NodeData, LocalServiceParam, TSettings);
}
```
**契约**：所有按节点查服务的逻辑必须走 `GetServiceOfNode`，缓存由 `ProviderCachedNodeServiceBase<>` 提供。

### 3.6 DI 注入契约（`InjectAttribute` + `ServiceProviderContractResolver`）

- 任何构造函数参数标记 `[Inject]` → 从 DI 容器解析
- Json 反序列化时遇到 `[Inject]` 参数 → 通过 `ServiceProviderContractResolver` 接管创建
- `PackedDataBase` 子类默认支持 DI 反序列化

---

## 四、主要数据流

### 4.1 启动加载流
```
App.xaml.cs OnStartup
  → SplashWindow.Show
  → new WPFApplicationHostBuilder(args).BuildHost()
      → ApplicationBootstrapLoader 创建临时 DI 容器
      → BootstrapLoaderNodePackageProvider 扫描 ./package/* 目录
      → ApplicationHostBuilder 注册 PackedServiceCollection、加载包程序集
      → NodePackageProvider 反射 [PackedServiceProvider] 类型 → 注册到对应 Provider
      → 反序列化 .<shortName> JSON 文件 → Register(id, packageInfo, data)
      → 应用 settings 覆盖默认值 (ReplaceSettingsForServiceShortNameIfValid)
  → NodePackageProvider.Register(SettingsDisplayDescriptorProvider / Docking* / ...)
  → MainWindow.Show
```

### 4.2 编辑流（用户拖拽新节点）
```
ToolboxViewModel.CreateNode
  → InsertCommandHostingService.CreateInsertCommand(doc, path, CreatedData)
  → CompositeCommand 包装 → CommandBuffer.Execute
  → EditorNode.Add(插入物理子节点) → 触发 OnChildrenChanged
  → NodeViewModel.HandleSourceChildrenChanged (订阅 ObservableCollection)
  → 插入新 NodeViewModel (DI: GetRequiredKeyedService<NodeViewModel>(ScopeKey.EditorNode))
  → WorkSpaceViewModel.BroadcastSelectedNodeChanged → PropertyView/Toolbox 重新解析
  → DocumentViewModel 标记 IsDirty
```

### 4.3 序列化流（保存 .lstgjson）
```
DocumentViewModel.SaveOrAskToSaveAs
  → Document.Save / SaveAs(path)
  → DocumentFormatBase (JSON / XML)
  → Newtonsoft.Json + ServiceProviderContractResolver
  → NodeData 树递归序列化 (TypeUID + Properties + Children)
```

### 4.4 代码生成 / 构建流
```
(用户触发 Build)
  → BuildTaskFactoryServiceProvider.GetServiceOfNode(rootNode)
      .CreateBuildingTask(node, context) → WeightedBuildingTask
  → CompositeBuildingTask 按 weight 串联
  → IBuildingTask.Execute(BuildingContext, IProgress<ProgressReportingParam>, CancellationToken)
      → 子任务内部：CodeGeneratorServiceProvider.GetServiceOfNode
          → CodeGenerationContext + Mustache 模板 → CodeData
          → ResourceGatheringServiceProvider 收集资源
  → 输出到 BuildingContext 临时目录 → DebugOutputPageViewModel
```

### 4.5 执行流（运行游戏）
```
WorkSpaceViewModel.ExecuteExecuteForSelected
  → Document.SaveOrAskToSaveAs
  → ExecutionConfigServiceProvider.GetExecutionConfigOfNode(node, param)
  → ExecutionTaskFactory(progressReporter, ct)  // 由包内 Execution 子模块提供
  → 启动外部 LuaSTG 进程，stdout 重定向到 IOutputLogWriter
```

---

## 五、核心调用链（按层向上）

| 起点 | 终点 | 链路 |
|---|---|---|
| 拖拽工具箱 | Core.Command | `ToolboxPageViewModel.CreateNode` → `InsertCommandHostingService` → `CompositeCommand` → `CommandBuffer.Execute` → `EditorNode.Add` |
| 选中节点 | PropertyView | `WorkSpaceViewModel.BroadcastSelectedNodeChanged` → `Anchorable.HandleSelectedNodeChanged` → `PropertyViewServiceProvider.GetPropertyViewModelOfNode` → `GetServiceOfNode` → 按 TypeUID 找 DataTemplate |
| 保存文档 | Core.Model | `DocumentViewModel.Save` → `Document.Save` → `DocumentFormatBase` → `NodeData` 树序列化 |
| Build | Core.Building | `WorkSpaceViewModel` → `BuildTaskFactoryServiceProvider` → `IBuildingTask.Execute` → `CodeGeneratorServiceProvider` → `MustacheCodeGeneration` |
| 启动 | 包加载 | `App.OnStartup` → `WPFApplicationHostBuilder` → `BootstrapLoaderNodePackageProvider` → `NodePackageProvider` → 反射 `[PackedServiceProvider]` + JSON 扫描 |

---

## 六、潜在技术债

### 6.1 架构 / 抽象层
1. ~~**Core.Command 引用 ViewModel**~~（已于 2026-06-27 修复）—— 原本 `src/LuaSTGEditorSharpV2.Core.Command/LuaSTGEditorSharpV2.Core.Command.csproj` 第 13 行直接引用上层 `ViewModel` 程序集，违反 C10。核查发现 9 处 `using LuaSTGEditorSharpV2.ViewModel;` 全为死引用（代码体内未使用任何 ViewModel 命名空间符号），故直接移除 csproj 中的 `ProjectReference` 并清理冗余 using。副作用：`LuaSTGEditorSharpV2.PropertyView` 此前通过 Core.Command → ViewModel 的传递引用间接使用 `AnchorableViewModelBase` / `LocalizableString` / `SelectedNodeChangedEventArgs` 等类型，已为其补上显式 `ProjectReference`。修复后命令栈测试 13/13 通过。
2. **PackedService 优先级用 `PriorityQueue<TData, PackageInfo>`，最小堆语义** —— `Priority` 数值**小**的实例在 `GetRegisteredAvailableData()` 中胜出（CORE 包 `Priority=0` 优先级最高）。命名易误导（直觉是"数值大=优先级高"），缺乏运行时可见的冲突诊断工具。
3. **节点遍历算法散落**：BFS/DFS、`ProceedChildren`、`UpdateViewModelDataRecursive` 在多个 ServiceProvider 中各自实现，没有统一 visitor 抽象。

### 6.2 包系统
4. **每个包需复制 5–9 个子 csproj**（LegacyNode 9 个、Lua 7 个），csproj 模板严重重复；新包门槛高。
5. **短名约定无强校验**：`.vm`/`.cgen`/`.build` 等扩展名靠约定，新增短名必须手工对齐文件扩展名与 `[ServiceShortName]` 特性，否则静默失败。
6. **PropertyView 跨包复制**：`SmoothSetValuePropertyViewItemTerm` 这类自定义属性编辑项在不同包里重复实现。
7. **`ServiceName` 与 `ServiceShortName` 命名风格不一致**：有的 Provider 同时声明两个，有的只声明短名（`resource`、`cboptions`、`sharpconv`），存在隐性约定。

### 6.3 UI 层
8. **`MainWindow.xaml.cs` 有大量 code-behind**（`LayoutItemTemplateSelector`、Ribbon 装配、布局序列化），View/ViewModel 边界模糊。
9. **`Dispatcher.Invoke` 散用**：缺统一的 UI 线程调度策略。
10. **事件订阅缺取消订阅**：`HandleSourceChildrenChanged` 等长期订阅容易在文档关闭后内存泄漏。
11. **`ServiceProvider.GetRequiredService<...>` 在多处用作服务定位器**，绕开构造函数注入。

### 6.4 资源 / 配置
12. **pack URI 字符串散布全代码**（如 `pack://application:,,,/LuaSTGEditorSharpV2.Package.LegacyNode.Resources.Shared;component/...`），程序集改名即全线失效。
13. **资源字典动态 Add 但缺 Remove**：主题切换的 `MergedDictionaries.Add` 长期累积。
14. **`./package/` 路径硬编码**（`BootstrapLoaderNodePackageProvider` 验证）。

### 6.5 测试 / 可观测性
15. `test/` 目录只有 `test.lstgjson` / `test.lstgxml` 数据样本，**没有任何单元测试工程**，命令系统、PackedService 加载、代码生成这些核心机制零回归保护。
16. 缺乏诊断日志（`LoggerHelper` 存在但使用密度低）。

---

## 七、后续改造必须遵守的工程约束

> 以下是从架构现有形态反推出的硬约束，违反任何一条都会破坏加载机制或破坏层间隔离。

### 7.1 PackedService 约束（最高优先级，破坏 = 包加载失败）
- **C1**：新增短名 Provider 时，**必须**同时声明 `[PackedServiceProvider]` + `[ServiceShortName("xxx")]` + `[ServiceName("YYY")]`，且短名对应的 JSON 文件扩展名必须与之一致（`.xxx`）。
- **C2**：JSON 文件**必须**放在 `./package/<packageName>/任意子目录/*.<shortName>`，文件内顶层 JSON 必须可反序列化为对应 `TData`，且若该 `TData` 有 `[Inject]` 参数，DI 容器必须能解析。
- **C3**：通过 `IServiceInstanceProvider<>` 提供的实例**必须**有零参构造（被反射 `Activator.CreateInstance` 调用）。
- **C4**：同名实例冲突时由 `PackageInfo.Priority` 决定胜出，**禁止**假设 "后注册者覆盖"。
- **C5**：UI 与 Package **禁止**静态/直接引用，所有交互只能经 PackedService + DI。

### 7.2 节点模型约束
- **C6**：所有节点属性值**必须**为 `string`，强类型在边界处用 `Property<T>` 包装；**禁止**直接改 `Dictionary<string,string>` 为其他类型（破坏序列化）。
- **C7**：`NodeData` 的 `PhysicalParent` **只能**由父节点回填（`[OnDeserialized]` 或插入逻辑）；外部代码**不得**直接写。
- **C8**：`IsActive == false` 的节点**必须**被所有"逻辑子节点"遍历跳过，但**必须**保留在物理子节点列表中（用于序列化保真）。
- **C9**：所有节点树变更**必须**经 `CommandBase` → `CommandBuffer`，否则撤销栈不一致；**禁止**直接调用 `EditorNode.Add` 绕过命令。

### 7.3 分层约束
- **C10**：Core 层**禁止**引用 ServiceBridge / UI / Package（包括 Transitively）。
- **C11**：Package 之间**禁止**互相引用；公共能力下沉到 Core 或独立的小工具包。
- **C12**：ServiceBridge**只能**做设置/适配，**禁止**承载业务逻辑或调用 Core 内部 API（只调公开 ServiceProvider）。
- **C13**：UI 层访问 Core 服务**必须**经 DI 容器或 ServiceBridge，**禁止** `new` Core 类型。

### 7.4 命令系统约束
- **C14**：每个 `CommandBase.Execute()` **必须**可被 `Revert()` 完整回滚；复合变更**必须**用 `CompositeCommand`，禁止多个独立命令分别 Execute。
- **C15**：结构性变更（增删节点、改 TypeUID）**必须**先经 `StructuralValidationServiceProvider.CanInsert / CanDeactivate` 校验，或包在 `CheckedCommand` 内。

### 7.5 序列化 / DI 约束
- **C16**：Json 反序列化涉及 `[Inject]` 参数的类型**必须**经 `ServiceProviderContractResolver`；**禁止**用默认 `JsonConvert.DeserializeObject` 直接反序列化带 DI 依赖的类型。
- **C17**：多态类型**必须**用 `[JsonTypeShortName]` 或 `$type` 显式声明，避免跨包加载时类型绑定失败。

### 7.6 资源 / 路径约束
- **C18**：pack URI 中引用的程序集名**禁止**重命名（除非同步更新所有引用点）；新增资源程序集**必须**沿用 `LuaSTGEditorSharpV2.*.Resources.Shared` 命名。
- **C19**：动态添加的 `ResourceDictionary` **必须**配套 Remove 路径，避免 `MergedDictionaries` 累积。
- **C20**：`./package/` 目录结构（`package/<name>/<sub>/<file>.<shortName>`）是 ABI，**禁止**改动；如需迁移必须保留兼容路径。

### 7.7 兼容性约束
- **C21**：`.lstgjson` / `.lstgxml` 是公开文件格式，**禁止**破坏向后兼容；新增字段必须有默认值；TypeUID 重命名必须靠 `SharpProjectConverter` 提供迁移路径。
- **C22**：新增短名 / Provider **必须**在本报告「3.3 服务短名映射表」中登记（当前为非强制约定，建议改为带 lint 校验）。

---

## 八、改造建议优先级（落地清单）

| 优先级 | 建议 | 预期收益 |
|---|---|---|
| ~~P0~~ 已完成 | 补充 Core.Command / PackedService / CodeGenerator 的单元测试工程（`test/LuaSTGEditorSharpV2.Core.Command.Tests` / `Core.Tests` / `Core.CodeGenerator.Tests`，xUnit + NSubstitute + 原生 Assert，共 27 个用例） | 拯救零回归保护的核心机制 |
| ~~P1~~ 已完成 | 抽出包模板 dotnet new template，固化 9-子模块结构（`templates/LuaSTGPackage/`，`dotnet new install ./templates/LuaSTGPackage`，短名 `luastg-package`，11 个 `--Include*` 布尔开关精细控制每个子模块是否生成） | 降低新包门槛，消除 csproj 重复 |
| P1 | 短名 lint 工具：检查 `[ServiceShortName]` ↔ 文件扩展名 ↔ 目录路径三方一致 | 消除静默失败 |
| P2 | 节点遍历抽出 `INodeVisitor` 抽象，收敛散落的 BFS/DFS | 改善可维护性 |
| P2 | pack URI 集中常量化 + 重命名安全网 | 降低改名风险 |
| P3 | `MainWindow.xaml.cs` code-behind 下沉到 ViewModel / Behaviors | 改善 MVVM 边界 |
| P3 | 事件订阅改 `CompositeDisposable` 模式 | 消除内存泄漏 |

---

## 附录：调研产出说明

架构图谱、调用链、短名表、约束清单均交叉验证过实际源码（`NodeData.cs`、`IPackedDataProviderService.cs`、`CommandBase.cs`、`CommandBuffer.cs`、`CompositeCommand.cs`、`PackedDataProviderServiceBase.cs`、`ServiceShortNameAttribute` 全仓库 grep、`test.lstgjson` 样例）。**P0 单元测试**（`test/LuaSTGEditorSharpV2.Core.Command.Tests` / `Core.Tests` / `Core.CodeGenerator.Tests`，xUnit + NSubstitute）已覆盖命令栈、复合命令回滚、`Commands` 工厂边界、`PackageInfo.CompareTo`、`PackedDataProviderServiceBase.Register` 优先级与重复检测、`CodeData` 行数、`CodeGenerationContext.GetIndented` 等核心路径，作为活文档验证本文描述。**P1 包模板**（`templates/LuaSTGPackage/`，使用说明见 `templates/LuaSTGPackage/README.md`）固化了 7 个核心 + 4 个可选子模块的 csproj 骨架（含 `OutputPath` 5-`..\` 路径约定、`CopyExtras` Target、`ExcludeAssets` 短名过滤、`manifest`/`Priority` 占位），新包通过 `dotnet new luastg-package -n <Name>` 一行生成。`MustacheCodeGeneration`、`CodeGenerationContext.AcquireContextHandle` 作用域、`PackedServiceCollection` 反射加载等需要完整 DI fixture 的场景留 P1。

> **维护者注意**：本文档是某时间点的架构快照。如代码演进与文档冲突，以代码为准并同步更新本文档，尤其是「3.3 服务短名映射表」与「第七章 工程约束」。
