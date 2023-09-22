# Introduction
## Three-layer Architecture
LuaSTG Editor Sharp V2 uses a 3-layer architecture, 
which is loose and highly modular.

The bottom layer is called _Core Layer_, which is
responsible for managing nodes, services and packages.
Project `LuaSTGEditorSharpV2.Core` declares the 
assembly for _Core Layer_. 

The medium layer is called _Service Layer_, which is
responsible for observing and processing data in nodes.
Each of the service may use independent assemblies, 
unless they need interactions. _Service Layer_ 
assemblies maybe platform specific, depending on the
requirements of _Application Layer_.

The top layer is called _Application Layer_, which is 
responsible for representing data and handling user
actions. Currently there are two _Application Layer_
assemblies, `LuaSTGEditorSharpV2` for windows GUI,
and `LuaSTGEditorSharpV2.CLI` for command line 
interface.

_Application Layer_ always only refer to the necessary 
services in _Service Layer_ in its life cycle.

## Basic concepts
### Service
In _Core Layer_, nodes only have its `type`, 
`children`, `properties` and whether `isbanned`.
Any of the code that watch or manipulate nodes will be
a service. There are many types of service. For
example, service responsible for code generation is 
called `CodeGenerationService`. All the service have
a base class called some `Service`. After declaring
a type of service, some concrete service derived form
the type of service should be defined. The service 
type will send the operation to a instance of a 
concrete service based on the type of the node.
Concrete service handles the actual work for specific
type of node. Instances of concrete services will be
serialized from the text file upon loading them.
### Package
Package is a collection of a group of concrete 
services. It includes the compiled assemblies of 
concrete services, and text file for declaring
instances of concrete services. Packages can be
load altogether, and instances can be unload 
altogether.