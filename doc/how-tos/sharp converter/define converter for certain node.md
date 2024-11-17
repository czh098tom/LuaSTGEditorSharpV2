# Writing `.sharpconv`

## Structure of `.sharpconv`

Each `.sharpconv` consists of a `SharpName` (the identifier of the node in Sharp V1) and a series of `ISharpNodeFormatConverter` operations (think of these as step-by-step conversion actions).

The `SharpName` refers to the `$type` key at the beginning of each line in the `.lstges` file from Sharp V1. You may notice that some identifiers are suffixed with "LuaSTGSharp" (likely to indicate an assembly name). Please retain this suffix.

The available converters are as follows:

### 1. **`Default`**
This includes two operations:
- **`StripNamespaceInType`**
- **`FormatProperty`**

(Refer to their descriptions below for details.)

### 2. **`StripNamespaceInType`**
For a `SharpName` like `.Group.NodeName, (LuaSTGSharp)`, this converter trims it to `NodeName`, which becomes the `TypeUID` of the node in V2.

### 3. **`FormatProperty`**
Processes properties of the original node as follows:
- Converts property names to lowercase.
- Replaces `(` with a space.
- Removes `)`, `'`.
- Converts `,` into a space.
- Replaces delimiters with underscores.

### 4. **`NodePropertyRemapping`**
Maps a property from an `OriginalKey` to a `NewKey` and removes the original key-value pair. Configurable fields:
- **`OriginalKey`**: The name of the property in Sharp V1.
- **`NewKey`**: The corresponding property name in Sharp V2.

### 5. **`NodeTypeRemapping`**
Changes the `TypeUID` of a node. Configurable field:
- **`To`**: The new `TypeUID`.

### 6. **`Composite`**
Combines multiple converters listed in the `Converters[]` field into a single composite converter.

---

## How to Write `.sharpconv`

### Example: Processing a Node `CreateBoss`

In Sharp V1, the node `CreateBoss` belongs to the "Boss" group, so its `$type` would likely be `".Boss.CreateBoss, "`. In V2, there is a node with the `TypeUID` of `CreateBoss`. To trim `".Boss.CreateBoss, "` to `CreateBoss`, the `.sharpconv` configuration would look like this:

```json
{
	"SharpName": ".Boss.CreateBoss, ",
	"Converters": [
		{ "$type": "StripNamespaceInType" }
	]
}
```

### Confirming Properties to Map

By checking the file `LuaSTGNode.Legacy/EditorData/Node/Boss/CreateBoss.cs`, we notice two properties, `Name` and `Wait`, marked with the `[NodeAttribute]` attribute. These correspond to node properties in Sharp V1.

Comparing this to `BuiltInPackages\LegacyNode\LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator\package\LegacyNode\Nodes\Boss\CreateBoss.cgen`, the `Captures` section reveals two corresponding properties: `identifier` and `wait`, though their names differ slightly. To map these properties, add `NodePropertyRemapping` converters:

```json
{
	"SharpName": ".Boss.CreateBoss, ",
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
}
```

---

### Special Cases

When working with the `BossInit` node, we find that properties like `Position` and `SCBG` in Sharp V1 are identical to `position` and `scbg` in Sharp V2, except for differences in capitalization. In such cases, the `FormatProperty` converter can be used to automatically format all properties in one step.

Additionally, the `Default` converter, which combines both `StripNamespaceInType` and `FormatProperty`, can simplify the configuration:

```json
{
	"SharpName": ".Boss.BossInit, ",
	"Converters": [
		{ "$type": "Default" }
	]
}
```