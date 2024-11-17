# 关于`.sharpconv`的编写

## 关于`.sharpconv`的结构

每个`.sharpconv`由一个`SharpName`（即节点在Sharp V1的标识符）和一系列的`ISharpNodeFormatConverter`（可视为一步转换操作）组成。

`SharpName`可以参考v1的.lstges文件中每行开头的`$type`键，你可能注意到有些节点的标识符后尾随"LuaSTGSharp"字样（似乎是用来标识程序集的），请保留。

其中Converter有以下几种：

### 1. `Default`：
包含`StripNamespaceInType`、`FormatProperty`两种操作，具体参考下文。

### 2. `StripNamespaceInType`：
将形如`.Group.NodeName, (LuaSTGSharp)`的`SharpName`，该Converter会将其裁剪为`NodeName`作为V2节点的`TypeUID`。

### 3. `FormatProperty`：
对原节点的属性进行如下处理：
- 将属性名转为小写。
- `(`转为空格。
- 去除`)`，`'`。
- `,`转为空格。
- 将分隔符转为下划线。

### 4. `NodePropertyRemapping`：
具有`OriginalKey`、`NewKey`两个可配置量，将原节点的`OriginalKey`属性映射到`NewKey`上并移除原键值对。

### 5. `NodeTypeRemapping`：
具有可配置量`To`，将`TypeUID`设置为`To`。

### 6. `Composite`：
具有可配置量`Converters[]`，将`Converters`内容组合为一个`Converter`（？）

## 所以要怎么写呢

比如说v1中有一个节点`CreateBoss`，属于Boss分组，所以其`$type`大约就是`".Boss.CreateBoss, "`，相应的，V2中有一个`TypeUID`为`CreateBoss`的节点，所以需要将`".Boss.CreateBoss, "`裁剪为`CreateBoss`，写成sharpconv配置就是
```json
{
	"SharpName": ".Boss.CreateBoss, ",
	"Converters": [
		{ "$type": "StripNamespaceInType" }
	]
}
```

### 确认有哪些属性需要映射

查看`LuaSTGNode.Legacy/EditorData/Node/Boss/CreateBoss.cs`，我们注意到有两个被`[NodeAttribute]`标注的属性`Name`、`Wait`，这就是SharpV1中的节点属性。

参考`BuiltInPackages\LegacyNode\LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator\package\LegacyNode\Nodes\Boss\CreateBoss.cgen`中的`Captures`可以发现恰有`identifier`，`wait`两个属性与之对应，只是名称略有不同，那么就需要向Converters中添加`NodePropertyRemapping`（属性映射转换器）
```json
	"Converters": [
		{ "$type": "StripNamespaceInType" },
		{
			"$type": "NodePropertyRemapping",
			"OriginalKey": "Name",
			"NewKey": "identifier"
		},
		{
			"$type": "NodePropertyRemapping",
			"OriginalKey": "Wait",
			"NewKey": "wait"
		}
	]
```

### 一些特别的情况

工作进行到`BossInit`这个节点时，我们发现V1的属性Position，SCBG与V2的属性position，scbg相比仅有大小写的区别，这时我们就可以使用`FormatProperty`转换器一键转换所有属性。

恰巧我们还有一个`Default`转换器可以一次执行`StripNamespaceInType`、`FormatProperty`两个操作，此时sharpconv的编写就变得非常清爽了
```json
{
	"SharpName": ".Boss.BossInit, ",
	"Converters": [
		{ "$type":"Default" }
	]
}
```