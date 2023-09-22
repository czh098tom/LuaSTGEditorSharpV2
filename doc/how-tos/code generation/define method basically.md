# How to: Define Code Generation Method for a Type of Node Basically
## 1. Identify packages to define
You should identify which package should include the 
definition of code generation for this node type.
There are two packages by default: `Core` and `LegacyNode`.
Package `Core` contains services for node stands 
for Lua language basics and infrastructure for the
editor.
Package `LegacyNode` contains services for node stands 
for LuaSTG Plus/EX/AEX.

If `Core` is selected, go to directory [here](/src/LuaSTGEditorSharpV2.Core.CodeGenerator/package/Core/Nodes).

If `LegacyNode` is selected, go to directory [here](/src/BuiltInPackages/LegacyNode/LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator/Nodes).

## 2. Write JSON to declare what to do with the node in code generation process
Create a file named `{yourNodeType}.cgen` at the directory selected.
Write the following code to that file:
```
{
  "$type": "LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.ConfigurableCodeGeneration, LuaSTGEditorSharpV2.Core.CodeGenerator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
}
```
which means it uses `ConfigurableCodeGeneration` class
when generating code.

Then add the following key-value pair to this json object:
```
"TypeUID": "{yourNodeType}"
```
This line tells the program to pass node with that 
`TypeUID` to the generator declared in this file.

The rest of this json object will declare what it will 
actually do when generating code. 

### Head and Tail
`ConfigurableCodeGeneration` uses key `Head` and 
`Tail` to declare the code generation result.
`Head` means code generated before the chilren of the
node, while `Tail` means code generated after the
children. You can use `{0}` to refer to the indention.

### Captures
`ConfigurableCodeGeneration` uses "Captures" to obtain 
values among node properties. "Captures" is a group of
json object which declares where to find and how to
pre-process those properties.
```
"Captures": [
    { "Capture": "{propertyName}" }
]
```
You can use the result of captures in `Head` and `Tail`
by using `{n}` where `n` is the index of capture
start with 1.

- e.g.
```
"Captures": [
  { "Capture": "condition" }
],
"Head": "elseif ({1}) then"
```
Tells it to capture the property named `condition` and
generate code `elseif (a) then` if `a` is written in
`condition` property in the node.