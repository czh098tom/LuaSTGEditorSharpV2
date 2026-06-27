# LuaSTG Editor V2 — Package Template

`dotnet new` template that scaffolds a LuaSTG Editor V2 built-in package with the standard
sub-module layout used by `LegacyNode`, `Lua`, and `LuaSTGSub`. Eliminates the csproj boilerplate
(PropertyGroup / `CopyExtras` Target / `ExcludeAssets` short-name filters) that every package
currently copy-pastes.

## Install (local template)

From the repository root:

```bash
dotnet new install ./templates/LuaSTGPackage --force
```

`--force` lets you re-run the command after editing the template. The template is registered
per-user (not in any project), so it persists across `dotnet new` invocations until you uninstall:

```bash
dotnet new uninstall LuaSTGEditorSharpV2.Package
```

## Use

Generate a new package into `src/BuiltInPackages/`:

```bash
dotnet new luastg-package -n MyPkg -o src/BuiltInPackages/MyPkg
```

This produces 7 default sub-projects (Main + Building + CodeGenerator + PropertyView + Toolbox +
ViewModel + Resources.Shared). Each sub-project is a self-contained csproj with the correct
`OutputPath`, `CopyExtras` Target, and `ExcludeAssets` short-name filter already wired up.

## Parameters

> The `dotnet new` templating engine uses the verbatim symbol name as the CLI flag — flags are
> PascalCase (e.g. `--IncludeExecution`), not kebab-case. Run `dotnet new luastg-package -h` to
> see the canonical list.

| Flag | Default | Effect |
|---|---|---|
| `-n <Name>` | (required) | PascalCase package name; substituted into csproj names, namespaces, manifest |
| `--Priority <float>` | `1.0` | `Priority` field in manifest. Lower wins in `PackedService` conflicts (built-ins span 1.0–2.0) |
| `--IncludeMain false` | `true` | Drop the main csproj (rarely useful) |
| `--IncludeBuilding false` | `true` | Drop `.resg` sub-module |
| `--IncludeCodeGenerator false` | `true` | Drop `.cgen` sub-module (also skips `PackageEntry.cs` skeleton) |
| `--IncludePropertyView false` | `true` | Drop `.prop` WPF sub-module |
| `--IncludeToolbox false` | `true` | Drop `.tool` WPF sub-module |
| `--IncludeViewModel false` | `true` | Drop `.vm` sub-module |
| `--IncludeResourcesShared false` | `true` | Drop the WPF resources/images sub-module |
| `--IncludeBuildTaskFactory` | `false` | Add `.build` sub-module |
| `--IncludeCLI` | `false` | Add CLIPluginProvider sub-module (registers CLI subcommands) |
| `--IncludeSharpConverter` | `false` | Add SharpProjectConverter sub-module (legacy `.sharpconv` migration) |
| `--IncludeExecution` | `false` | Add `.execfg` sub-module (launches external LuaSTG process) |
| `--AuthorName <string>` | `LuaSTGEditorSharpV2` | Author tag (currently unused by the build but reserved for future NuGet packaging) |

For boolean flags with default `true`, pass `--FlagName false` to disable. For flags with default
`false`, pass `--FlagName` (or `--FlagName true`) to enable.

### Example: minimal LuaSTGSub-style package

```bash
dotnet new luastg-package -n MyExecutor \
  --IncludeBuilding false --IncludeCodeGenerator false \
  --IncludeToolbox false --IncludeViewModel false --IncludeResourcesShared false \
  --IncludeExecution \
  -o src/BuiltInPackages/MyExecutor
```

Produces 3 csprojs: Main + PropertyView + Execution.

### Known limitation: Toolbox requires Resources.Shared

The Toolbox sub-module's csproj has a hardcoded `<ProjectReference>` to
`LuaSTGEditorSharpV2.Package.<Name>.Resources.Shared`. If you pass
`--IncludeToolbox` (default) together with `--IncludeResourcesShared false`, the
generated Toolbox csproj will reference a non-existent project and the build
will emit `warning MSB9008`. To work around this, either keep
`--IncludeResourcesShared` enabled (default), or open the generated Toolbox
csproj and delete the `<ProjectReference Include="...Resources.Shared..." />`
line.

### Example: full LegacyNode-style package (all 11 sub-modules)

```bash
dotnet new luastg-package -n MyFull \
  --IncludeBuildTaskFactory --IncludeCLI \
  --IncludeSharpConverter --IncludeExecution \
  -o src/BuiltInPackages/MyFull
```

## After generation

1. **Register the new csprojs in the solution.** Open `LuaSTGEditorSharpV2.slnx` and add a new
   `<Folder>` block under `/src/BuiltInPackages/` listing each generated csproj. The slnx format
   is XML; copy the structure of the existing `/src/BuiltInPackages/LegacyNode/` entry.

2. **Build the solution** to surface the new package at
   `bin/$(Configuration)/package/<Name>/`:
   ```bash
   dotnet build LuaSTGEditorSharpV2.slnx
   ```

3. **Drop JSON data files** into each sub-module's `package/<Name>/...` directory. The `.gitkeep`
   placeholder files document which extension (`.cgen`, `.vm`, `.prop`, ...) goes where. Delete
   the `.gitkeep` once real files land.

4. **(Optional) Implement ServiceProvider classes.** The CodeGenerator sub-module ships with a
   working `PackageEntry.cs` + `SampleLanguage.cs` skeleton; rename `SampleLanguage` and adjust
   `Name` to match your language. Other sub-modules start empty — add ServiceProvider subclasses
   decorated with `[PackedServiceProvider]` + `[ServiceShortName("xxx")]` as needed.

## Conventions encoded by this template

The template enforces several invariants documented in `doc/Architecture.md`:

- **C18 pack-URI assembly names**: csproj names follow `LuaSTGEditorSharpV2.Package.<Name>.<Sub>`.
- **C20 package directory ABI**: every csproj writes to
  `bin/$(Configuration)/package/<Name>/` via the 5-`..\` OutputPath pattern.
- **ExcludeAssets short-name filters**: each sub-module excludes the JSON file extension it
  owns (`.cgen` / `.prop` / `.vm` / `.tool` / `.build` / `.resg` / `.execfg` / `.sharpconv`),
  preventing the project-reference build from copying another package's data files.
- **`Private=false` on all project references**: keeps the package output directory clean —
  only this package's DLL lands in `bin/$(Configuration)/package/<Name>/`.

See `doc/Architecture.md` §3.3 (Service Short Name mapping) and §7 (engineering constraints)
for the full contract.
