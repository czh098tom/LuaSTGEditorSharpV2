# Introduction
## Three-layer Architecture
LuaSTG Editor Sharp V2 uses a 3-layer architecture, 
which is loosly coupled and highly modular.

### Core Layer
The bottom layer is called _Core Layer_, which is
responsible for managing nodes and application lifecycle.
Project `LuaSTGEditorSharpV2.Core` declares the 
assembly for _Core Layer_. 

### Service Layer
The medium layer is called _Service Layer_, which is
responsible for observing and processing data.
Each of the service may use independent assemblies, 
unless they need references. _Service Layer_ 
assemblies maybe platform specific, depending on the
references of _Application Layer_.

### Application Layer
The top layer is called _Application Layer_, which is 
responsible for representing data and handling user
actions. Currently there are two _Application Layer_
assemblies, `LuaSTGEditorSharpV2` for windows GUI,
and `LuaSTGEditorSharpV2.CLI` for command line 
interface.

_Application Layer_ always refer to the fewest necessary 
services in _Service Layer_ in its life cycle.

## Basic concepts
### Core Module
Any assemblies supports basic functions of editor
is considered as core module.

### Packed Services
Services providing information or logic based on 
packages are called packed services. 

Packed services need instances to work.
Packed services instances are instantiated from 
packages, then loaded inside its packed service provider. 
Packed service provider classes will be 
derived from `PackedDataProviderServiceBase<>`, and 
packed service classes will be derived from 
`PackedDataBase`.
Packed service instances will be created in two ways:

1. If `ServiceShortNameAttribute` is defined on its 
   provider, 
   `./package/<package_name>/*/*.<ServiceShortNameAttribute.Name>`
   is scanned, any file satisfies this criteria will be 
   considered as json object of packed service instances.
2. Any classes in loaded assemblies implements
   interface `IServiceInstanceProvider<>` of
   declared packed services will be
   created using its default constructor. Then
   `GetServiceInstances` method will be called
   on instance to obtain packed service instances.
   This should only be used if they are highly
   depend on assembly resources and will never
   change by user upon publication.

Instances of packed services are keyed and can be
later fetched by its key in _Application Layer_ or 
referenced _Service Layer_ assemblies.

Packed services and its providers can be declared in
core module or packages.

### Package
Package is a collection of functionality that can be
used in editor. It is a _Service Layer_ architecture.

Once application starts, packages declared in its 
settings will be loaded. Packages are loaded in two
steps:

1. Load assemblies searching name
   `./package/<package_name>/LuaSTGEditorSharpV2.Package.<package_name>.<ServiceNameAttribute.Name>.dll`
   for all packed service providers with 
   `ServiceNameAttribute` declaring in core module.
2. Load packed services instances based on all loaded
   packed services in currently loaded assemblies.

### Dependency Injection
LuaSTG Editor Sharp V2 uses DI to manage lifecycle and
dependencies of packed services. `InjectAttribute` can
be used for any classes to register as serices.

**Note: Dependency injection services is a different
concept comparing with packed service concept above.** 

By default, packed services supports dependency 
injection when doing json deserialization.