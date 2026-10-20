using System.Reflection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Specialized.Curve;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Tests;

public sealed class CurveCodeGenerationTests
{
    [Fact]
    public void AssetsLoadDefaultsAndAllDeclarationsPrecedeTasks()
    {
        using var fixture = new Fixture();
        var track = Track("target.x={0}; target.y={1}");
        var first = Curve("2", "Round Robin", Point("0", "10"), Point("3", "20", "easeOut", "true"));
        var second = Curve("5", "None", Point("0", "30"), Point("4", "40"));
        track.Add(first); track.Add(Node("marker")); track.Add(second);
        var output = fixture.Generate(track);
        var text = string.Concat(output.Select(item => item.Content));
        Assert.IsType<NumericalTrackCodeGeneration>(fixture.Service(track));
        Assert.IsType<TemplateNumericalCurveCodeGeneration>(fixture.Service(first));
        Assert.IsType<TemplateCurvePointCodeGeneration>(fixture.Service(first.PhysicalChildren[0]));
        Assert.Equal("0", fixture.Default(Node("NumericalPoint"), "time"));
        Assert.Equal("self", fixture.Default(Node("NumericalTrack"), "target"));
        Assert.Equal("None", fixture.Default(Node("NumericalCurve"), "repeat_type"));
        Assert.True(text.IndexOf("local _0 = 10", StringComparison.Ordinal) < text.IndexOf("task.New(target, function()", StringComparison.Ordinal));
        Assert.True(text.IndexOf("local _1 = 30", StringComparison.Ordinal) < text.IndexOf("task.New(target, function()", StringComparison.Ordinal));
        Assert.Contains("target.x=_0; target.y=_1", text);
        Assert.Contains("prev + ky * easeOut((i - ibeg) / kx)", text);
        Assert.Equal(12, Assert.Single(output.Where(item => ReferenceEquals(item.Source, first.PhysicalChildren[1]))).LineCount);
    }

    [Theory]
    [InlineData("0", true, "disabled")]
    [InlineData("1", false, "zero")]
    public void InvalidFirstPhysicalPointReportsItsSource(string time, bool disabled, string message)
    {
        using var fixture = new Fixture();
        var first = Point(time, "0"); first.IsActive = !disabled;
        var curve = Curve("0", "None", first, Point("0", "1"));
        var error = Assert.Throws<CurveGenerationException>(() => fixture.Generate(Track("", curve)));
        Assert.Same(first, error.SourceNode);
        Assert.Contains(message, error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LaterPointExpressionsRawInitialValuesAndMacroPreviousValuesRemainDistinct()
    {
        using var fixture = new Fixture();
        var macro = Node("macro", ("replace", "VALUE"), ("by", "VALUE+1"));
        var curve = Curve("0", "None", Point("0", "VALUE"), Point("i + 3", "20"));
        macro.Add(Track("", curve));
        var text = string.Concat(fixture.Generate(macro).Select(item => item.Content));
        Assert.Contains("local _0 = VALUE\n", text);
        Assert.Contains("local prev = VALUE+1", text);
        Assert.DoesNotContain("VALUE+1+1", text);
        Assert.Contains("target = i + 3 + offset", text);
    }

    [Theory]
    [InlineData("None", "false", "false")]
    [InlineData("Round Robin", "false", "true")]
    [InlineData("None", "true", "false")]
    [InlineData("Round Robin", "true", "true")]
    public void OffsetRoundRobinAndRelativePointCombinationsUseConfiguredTemplates(string repeatType, string relative, string loop)
    {
        using var fixture = new Fixture();
        var track = Track("", Curve("4", repeatType, Point("0", "1"), Point("2", "3", " ", relative)));
        var text = string.Concat(fixture.Generate(track).Select(item => item.Content));
        Assert.Contains(relative == "false" ? "target = 2 + offset" : "target = target + 2", text);
        Assert.Equal(loop == "true" ? 2 : 1,
            text.Split("for _ = 1, _infinite do", StringSplitOptions.None).Length - 1);
        Assert.Contains("task.Wait(4)", text);
        Assert.Contains("prev + ky * (i - ibeg) / kx", text);
    }

    [Fact]
    public void DisabledCurvesMappingFormatsAndAutoTerminateUseTheSameCurveList()
    {
        using var fixture = new Fixture();
        var ignored = Curve("0", "None", Point("0", "9")); ignored.IsActive = false;
        var track = Track("{{0}} = {0}; {1}", Curve("0", "None", Point("0", "1")), ignored, Curve("0", "None", Point("0", "2")));
        var text = string.Concat(fixture.Generate(track).Select(item => item.Content));
        Assert.Contains("{0} = _0; _1", text);
        Assert.Contains("__terminateCount >= 2", text);
        track.Properties["mapping"] = "{2}";
        Assert.Throws<FormatException>(() => fixture.Generate(track));
    }

    [Fact]
    public void LeafStatementsAndTrackScopeArePreserved()
    {
        using var fixture = new Fixture();
        var point = Point("0", "1");
        var curve = Curve("0", "None", point, Node("SkipTime", ("time", "2"), ("is_relative", "true")), Node("TerminateTrack"));
        fixture.Register("customCurve", new ProbeCurve(fixture.Generator, fixture.Services));
        var custom = Node("customCurve");
        var outer = Track("", curve, custom); var parent = Node("parent"); parent.Add(outer);
        using var scope = fixture.Context.AcquireContextHandle(parent, 2);
        var code = fixture.Generate(outer);
        var text = string.Concat(code.Select(item => item.Content));
        Assert.Contains("i = i + 2", text);
        Assert.Contains("__terminated = true", text);
        Assert.Contains("probe-declaration", text);
        Assert.Contains("probe-task", text);
        Assert.Same(parent, fixture.Context.Peek());
        Assert.Equal(2, fixture.Context.IndentionLevel);
    }

    [Fact]
    public void EmptyTrackAndMissingFirstPointHaveIndependentBehavior()
    {
        using var fixture = new Fixture();
        Assert.Contains("__terminateCount >= 0", string.Concat(fixture.Generate(Track("")).Select(item => item.Content)));
        var curve = Curve("0", "None", Node("marker"));
        var error = Assert.Throws<CurveGenerationException>(() => fixture.Generate(Track("", curve)));
        Assert.Same(curve, error.SourceNode);
    }

    [Fact]
    public void DisabledLaterPointIsSkippedWithoutChangingTheFirstPoint()
    {
        using var fixture = new Fixture();
        var later = Point("8", "99");
        later.IsActive = false;
        var track = Track("", Curve("0", "None", Point("0", "1"), later));
        var code = string.Concat(fixture.Generate(track).Select(item => item.Content));
        Assert.DoesNotContain("99", code);
        Assert.Contains("local _0 = 1", code);
    }

    [Fact]
    public void PointAndSkipTimeUseTheirDistinctExactRelativeFlags()
    {
        using var fixture = new Fixture();
        var track = Track("", Curve("0", "None", Point("0", "1"), Point("2", "2", "", "TRUE"),
            Node("SkipTime", ("time", "3"), ("is_relative", "TRUE"))));
        var code = string.Concat(fixture.Generate(track).Select(item => item.Content));
        Assert.Contains("target = target + 2", code);
        Assert.Contains("i = 3", code);
    }

    [Fact]
    public void MappingEscapesBracesAndBadIndexesPropagateFormatException()
    {
        using var fixture = new Fixture();
        var track = Track("{{0}}={0}", Curve("0", "None", Point("0", "1")));
        Assert.Contains("{0}=_0", string.Concat(fixture.Generate(track).Select(item => item.Content)));
        track.Properties["mapping"] = "{9}";
        Assert.Throws<FormatException>(() => fixture.Generate(track));
    }

    [Fact]
    public void MarkerAndProbeProtocolsPreserveOrderSourceAndScope()
    {
        using var fixture = new Fixture();
        var probePoint = new ProbePoint(fixture.Generator, fixture.Services);
        fixture.Register("customPoint", probePoint);
        var marker = Node("marker");
        var point = Node("customPoint");
        var curve = Curve("0", "None", point);
        var track = Track("", marker, curve);
        var code = fixture.Generate(track);
        var declarationIndex = Array.FindIndex(code, item => ReferenceEquals(item.Source, curve)
            && item.Content.Contains("local _0 = 0", StringComparison.Ordinal));
        var markerIndex = Array.FindIndex(code, item => ReferenceEquals(item.Source, marker));
        var taskIndex = Array.FindIndex(code, item => ReferenceEquals(item.Source, curve)
            && item.Content.Contains("task.New", StringComparison.Ordinal));
        Assert.True(declarationIndex >= 0 && declarationIndex < markerIndex && markerIndex < taskIndex);
        Assert.Equal(1, probePoint.Calls);
        Assert.Same(curve, probePoint.CurveAncestor);
        Assert.Same(point, Assert.Single(code.Where(item => ReferenceEquals(item.Source, point))).Source);
    }

    [Fact]
    public void ThrowingCurvePropagatesOriginalExceptionAndNullPointStillReportsItsSource()
    {
        using var fixture = new Fixture();
        var throwing = Node("throwingCurve");
        var service = new ThrowingCurve(fixture.Generator, fixture.Services);
        fixture.Register("throwingCurve", service);
        var track = Track("", throwing);
        var parent = Node("parent");
        parent.Add(track);
        using var scope = fixture.Context.AcquireContextHandle(parent, 2);

        Assert.Same(service.Error, Assert.Throws<InvalidOperationException>(() => fixture.Generate(track)));
        Assert.Same(parent, fixture.Context.Peek());
        Assert.Equal(2, fixture.Context.IndentionLevel);
        Assert.Single(fixture.Context.ContextVariables);
        var nullPoint = Node("nullPoint");
        fixture.Register("nullPoint", new NullPoint(fixture.Generator, fixture.Services));
        Assert.Same(nullPoint, Assert.Throws<CurveGenerationException>(() => fixture.Generate(Track("", Curve("0", "None", nullPoint)))).SourceNode);
    }

    [Fact]
    public void OrdinaryNodesRemainBetweenTheirNeighboringPoints()
    {
        using var fixture = new Fixture();
        var first = Point("0", "1");
        var marker = Node("marker");
        var last = Point("2", "3");
        var code = fixture.Generate(Track("", Curve("0", "None", first, marker, last)));
        var firstIndex = Array.FindIndex(code, item => ReferenceEquals(item.Source, first));
        var markerIndex = Array.FindIndex(code, item => ReferenceEquals(item.Source, marker));
        var lastIndex = Array.FindIndex(code, item => ReferenceEquals(item.Source, last));

        Assert.True(firstIndex >= 0 && firstIndex < markerIndex && markerIndex < lastIndex);
        Assert.Single(code.Where(item => ReferenceEquals(item.Source, marker)));
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("1 == 1", true)]
    public void AutoTerminateUsesEvaluatedBoolean(string flag, bool expected)
    {
        using var fixture = new Fixture();
        var track = Track("", Curve("0", "None", Point("0", "1")));
        track.Properties["auto_terminate"] = flag;
        var text = string.Concat(fixture.Generate(track).Select(item => item.Content));

        Assert.Equal(expected, text.Contains(" or __terminateCount >= 1", StringComparison.Ordinal));
        Assert.Contains("if __terminated", text);
    }

    [Fact]
    public void MissingNodePropertiesUseDefaultsDuringGeneration()
    {
        using var fixture = new Fixture();
        var track = Node("NumericalTrack");
        var curve = Node("NumericalCurve");
        curve.Add(Node("NumericalPoint"));
        track.Add(curve);
        var text = string.Concat(fixture.Generate(track).Select(item => item.Content));

        Assert.Contains("local _0 = 0", text);
        Assert.Contains("task.New(self, function()", text);
        Assert.Contains("task.Wait(0)", text);
        Assert.Contains("target = 0 + offset", text);
    }

    [Fact]
    public void CustomCurveRunsOnceAndMaterializesBeforeEnumeration()
    {
        using var fixture = new Fixture();
        var probe = new ProbeCurve(fixture.Generator, fixture.Services);
        fixture.Register("customCurve", probe);
        var curve = Node("customCurve");
        var track = Track("value={0}", curve);
        var parent = Node("parent");
        parent.Add(track);
        using var scope = fixture.Context.AcquireContextHandle(parent, 2);

        var generated = fixture.Generator.GenerateCode(track, fixture.Context);
        Assert.Equal(1, probe.Calls);
        Assert.Equal(2, probe.Materializations);
        Assert.Equal(new[] { parent, track }, probe.Ancestors);
        Assert.Equal("_0", probe.VariableName);
        Assert.Same(parent, fixture.Context.Peek());
        Assert.Equal(2, fixture.Context.IndentionLevel);
        using (var iterator = generated.GetEnumerator())
        {
            Assert.True(iterator.MoveNext());
        }
        var saved = generated.ToArray();
        Assert.Equal(1, probe.Calls);
        Assert.Equal(2, probe.Materializations);
        Assert.Equal(new[] { "   probe-declaration\n", "   probe-task\n" },
            saved.Where(item => ReferenceEquals(item.Source, curve)).Select(item => item.Content));
        Assert.Same(parent, fixture.Context.Peek());
    }

    [Fact]
    public void NestedTracksKeepTheirNearestTargetAndRestoreCallerScope()
    {
        using var fixture = new Fixture();
        var inner = Track("", Curve("0", "None", Point("0", "7")));
        inner.Properties["target"] = "inner_target";
        var outer = Track("", Curve("0", "None", Point("0", "1")), inner);
        outer.Properties["target"] = "outer_target";
        var parent = Node("parent");
        parent.Add(outer);
        using var scope = fixture.Context.AcquireContextHandle(parent, 1);
        var text = string.Concat(fixture.Generate(outer).Select(item => item.Content));

        Assert.Contains("  task.New(outer_target, function()", text);
        Assert.Contains("   task.New(inner_target, function()", text);
        Assert.Same(parent, fixture.Context.Peek());
        Assert.Equal(1, fixture.Context.IndentionLevel);
        Assert.Single(fixture.Context.ContextVariables);
    }

    [Fact]
    public void PointFailureRestoresBothCurveAndTrackScopes()
    {
        using var fixture = new Fixture();
        var point = Point("x", "0");
        var track = Track("", Curve("0", "Round Robin", point));
        var parent = Node("parent");
        parent.Add(track);
        using var scope = fixture.Context.AcquireContextHandle(parent, 3);

        Assert.Throws<FormatException>(() => fixture.Generate(track));
        Assert.Same(parent, fixture.Context.Peek());
        Assert.Equal(3, fixture.Context.IndentionLevel);
        Assert.Single(fixture.Context.ContextVariables);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OrdinaryChildFailurePropagatesOriginalExceptionAndRestoresCallerScope(bool insideCurve)
    {
        using var fixture = new Fixture();
        var failure = new InvalidOperationException("ordinary child failure");
        fixture.Register("throwingStatement", new ThrowingStatement(fixture.Generator, fixture.Services, failure));
        var child = Node("throwingStatement");
        var track = insideCurve
            ? Track("", Curve("0", "None", Point("0", "1"), child))
            : Track("", child);
        var parent = Node("parent");
        parent.Add(track);
        using var scope = fixture.Context.AcquireContextHandle(parent, 3);

        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => fixture.Generate(track)));
        Assert.Same(parent, fixture.Context.Peek());
        Assert.Equal(3, fixture.Context.IndentionLevel);
        Assert.Single(fixture.Context.ContextVariables);
    }

    [Fact]
    public void TrackCapturesNamingAndPhaseTemplatesRemainConfigurable()
    {
        using var fixture = new Fixture();
        var track = Track("unused", Curve("0", "None", Point("0", "1")));
        track.Properties["custom_mapping"] = "{{literal}}={0}";
        var service = (NumericalTrackCodeGeneration)fixture.Service(track);
        service.MappingCapture = JsonConvert.DeserializeObject<CaptureWithMacroOption>(
            """{ "Key": "custom_mapping", "DisableMacro": true }""")!;
        service.VariableNameFormat = "custom_{0}";
        service.ConditionTemplates["hasCurve"] = "{{args.curveCount}} == 1";
        service.Head = ["hea", "d{{#condition.hasCurve}}1{{/condition.hasCurve}}"];
        service.MappingTask = ["{{{args.mapping}}}"];
        service.Tail = [];
        var code = fixture.Generate(track);

        Assert.Equal("head1\n", code[0].Content);
        Assert.Equal("{literal}=custom_0\n", code[^1].Content);
        Assert.Contains("local custom_0 = 1", string.Concat(code.Select(item => item.Content)));
    }

    [Fact]
    public void CurveTemplatesShareOneCaptureAndDoNotCacheAcrossCalls()
    {
        using var fixture = new Fixture();
        var curve = Node("countedCurve");
        curve.Add(Point("0", "1"));
        var track = Track("", curve);
        var defaults = new CountingDefaults(fixture.Services);
        fixture.RegisterDefaults("countedCurve", defaults);
        var service = new TemplateNumericalCurveCodeGeneration(fixture.Generator, fixture.Services);
        fixture.Register("countedCurve", service);
        service.Captures = JsonConvert.DeserializeObject<Dictionary<string, CaptureWithMacroOption>>(
            """{ "probe": { "Key": "probe", "DisableMacro": true } }""")!;
        service.ContextCaptures.Clear();
        service.ConditionTemplates = new() { ["first"] = "'{{self.probe}}' == 'value-1'" };
        service.Declaration = ["{{{self.probe}}}"];
        service.TaskHead = ["{{#condition.first}}first{{/condition.first}}{{^condition.first}}next{{/condition.first}}"];
        service.BodyIndention = ["1"];
        service.BodyHead = [];
        service.BodyTail = [];
        service.TaskTail = ["{{{self.probe}}}"];

        var first = string.Concat(fixture.Generate(track).Select(item => item.Content));
        var second = string.Concat(fixture.Generate(track).Select(item => item.Content));
        Assert.Equal(2, defaults.Calls);
        Assert.Equal(2, first.Split("value-1", StringSplitOptions.None).Length - 1);
        Assert.Equal(2, second.Split("value-2", StringSplitOptions.None).Length - 1);
        Assert.Contains(" first\n", first);
        Assert.Contains(" next\n", second);
    }

    [Fact]
    public void RenderTextDoesNotAddIndentNewlinesOrReinterpretCapturedTokens()
    {
        using var fixture = new Fixture();
        var node = Node("point");
        using var scope = fixture.Context.AcquireContextHandle(Node("parent"), 3);
        var model = CodeFragmentTemplate.CreateModel(fixture.Services, node, fixture.Context,
            new Dictionary<string, CaptureWithMacroOption>(), new Dictionary<string, ContextCapture>(),
            new Dictionary<string, string>(), new Dictionary<string, object?> { ["value"] = "{0:IND}<>&" });
        string[] template = ["{0:", "IND}{{{args.value}}}"];

        Assert.Equal(" {0:IND}<>&", CodeFragmentTemplate.RenderText(template, model, fixture.Context));
        Assert.Equal("    {0:IND}<>&\n", CodeFragmentTemplate.Render(template, model, node, fixture.Context)[0].Content);
        Assert.Equal("", CodeFragmentTemplate.RenderText([], model, fixture.Context));
        Assert.Empty(CodeFragmentTemplate.Render([], model, node, fixture.Context));
        Assert.Throws<ArgumentNullException>(() => CodeFragmentTemplate.RenderText(null!, model, fixture.Context));
        Assert.Throws<ArgumentException>(() => CodeFragmentTemplate.RenderText([null!], model, fixture.Context));
    }

    [Fact]
    public void CurveAndPointSubservicesAreSilentInOrdinaryTraversal()
    {
        using var fixture = new Fixture();
        var curve = Curve("0", "None", Point("0", "1"));
        Assert.Empty(fixture.Generate(curve));
        Assert.Empty(fixture.Generate(curve.PhysicalChildren[0]));
    }

    private static NodeData Node(string type, params (string Key, string Value)[] properties)
    {
        var node = new NodeData(type);
        foreach (var (key, value) in properties)
        {
            node.Properties[key] = value;
        }
        return node;
    }

    private static NodeData Point(string time, string value, string interpolation = "", string relative = "false")
        => Node("NumericalPoint", ("time", time), ("value", value),
            ("interpolation", interpolation), ("is_relative", relative));

    private static NodeData Curve(string offset, string repeat, params NodeData[] children)
    {
        var node = Node("NumericalCurve", ("offset", offset), ("repeat_type", repeat));
        foreach (var child in children)
        {
            node.Add(child);
        }
        return node;
    }

    private static NodeData Track(string mapping, params NodeData[] children)
    {
        var node = Node("NumericalTrack", ("target", "target"), ("mapping", mapping), ("auto_terminate", "true"));
        foreach (var child in children)
        {
            node.Add(child);
        }
        return node;
    }

    private sealed class Fixture : IDisposable
    {
        private static readonly PackageManifest Manifest = new("LegacyNode", new Version(1, 0), 2f, null);
        private readonly List<IDisposable> registrations = [];
        public ServiceProvider Services { get; }
        public CodeGeneratorServiceProvider Generator => Services.GetRequiredService<CodeGeneratorServiceProvider>();
        public CodeGenerationContext Context { get; }
        public Fixture()
        {
            var services = new ServiceCollection()
                .AddSingleton<LanguageProviderService>()
                .AddSingleton<SerializationTypeBindingService>()
                .AddSingleton<CodeGeneratorServiceProvider>()
                .AddSingleton<DefaultValueServiceProvider>();
            services.Add(typeof(DefaultValueService).GetCustomAttribute<InjectAttribute>()!.ToDescriptor(typeof(DefaultValueService)));
            foreach (var type in new[]
            {
                typeof(CodeGeneratorServiceBase).Assembly, typeof(NumericalTrackCodeGeneration).Assembly
            }.SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && typeof(CodeGeneratorServiceBase).IsAssignableFrom(type)))
            {
                services.Add(type.GetCustomAttribute<InjectAttribute>()!.ToDescriptor(type));
            }
            Services = services.BuildServiceProvider();
            Context = new(Services, new LocalServiceParam(null!), new CodeGenerationServiceSettings { IndentionString = " " });
            registrations.Add(Services.GetRequiredService<LanguageProviderService>().Register("Lua", new PackageInfo(Manifest, ""), new TestLanguage(Services)));
            Register("marker", new Marker(Generator, Services));
            var packed = new PackedServiceCollection();
            packed.Add<CodeGeneratorServiceProvider>();
            packed.Add<DefaultValueServiceProvider>();
            var loader = new NodePackageProvider(NullLogger<NodePackageProvider>.Instance, packed, Services);
            registrations.Add(loader.LoadPackageFromDirectory(new PackageAssemblyDescriptor("LegacyNode", Path.Combine(AppContext.BaseDirectory, "package", "LegacyNode"), Manifest, [typeof(NumericalTrackCodeGeneration).Assembly])));
        }
        public CodeGeneratorServiceBase Service(NodeData node) => Generator.ResolveService(node);
        public string Default(NodeData node, string key) => new NodePropertyAccessToken(Services, node, Context).GetValueWithDefault(key);
        public CodeData[] Generate(NodeData node) => Generator.GenerateCode(node, Context).ToArray();
        public void Register(string type, CodeGeneratorServiceBase service) => registrations.Add(Generator.Register(type, new PackageInfo(Manifest, ""), service));
        public void RegisterDefaults(string type, DefaultValueService service)
            => registrations.Add(Services.GetRequiredService<DefaultValueServiceProvider>()
                .Register(type, new PackageInfo(Manifest, ""), service));
        public void Dispose()
        {
            foreach (var item in registrations.AsEnumerable().Reverse())
            {
                item.Dispose();
            }
            Services.Dispose();
        }
    }
    private sealed class TestLanguage : LanguageBase
    {
        public override string Name => "Lua";
        public TestLanguage(IServiceProvider services) : base(services)
        {
            MacroTemplate = @"\b{0}\b";
        }
    }
    private sealed class ProbeCurve(CodeGeneratorServiceProvider provider, IServiceProvider services) : NumericalCurveCodeGeneration(provider, services)
    {
        public int Calls { get; private set; }
        public int Materializations { get; private set; }
        public string? VariableName { get; private set; }
        public NodeData[] Ancestors { get; private set; } = [];

        public override CurveOutput Generate(NodeData node, CodeGenerationContext context, string name)
        {
            Calls++;
            VariableName = name;
            Ancestors = context.EnumerateFromFarest().ToArray();
            return new CurveOutput(Emit("probe-declaration", node, context), Emit("probe-task", node, context));
        }

        private IEnumerable<CodeData> Emit(string text, NodeData node, CodeGenerationContext context)
        {
            Materializations++;
            yield return new CodeData(context.ApplyIndentedFormat(text).ToString(), node);
        }
    }
    private sealed class ProbePoint(CodeGeneratorServiceProvider provider, IServiceProvider services) : CurvePointCodeGeneration(provider, services)
    {
        public int Calls { get; private set; }
        public NodeData? CurveAncestor { get; private set; }
        public override CurvePointOutput Generate(NodeData node, CodeGenerationContext context, string name, bool first)
        {
            Calls++;
            CurveAncestor = context.PeekType("NumericalCurve");
            return new(first ? "0" : null, first ? "0" : null, [new CodeData(context.ApplyIndentedFormat("probe-point").ToString(), node)]);
        }
    }
    private sealed class Marker(CodeGeneratorServiceProvider provider, IServiceProvider services) : CodeGeneratorServiceBase(provider, services)
    {
        protected override IEnumerable<CodeData> GenerateCodeWithContext(NodeData node, CodeGenerationContext context) => [new CodeData(context.ApplyIndentedFormat("marker").ToString(), node)];
    }
    private sealed class ThrowingCurve(CodeGeneratorServiceProvider provider, IServiceProvider services) : NumericalCurveCodeGeneration(provider, services)
    {
        public InvalidOperationException Error { get; } = new("expected");

        public override CurveOutput Generate(NodeData node, CodeGenerationContext context, string name) => throw Error;
    }
    private sealed class ThrowingStatement(
        CodeGeneratorServiceProvider provider, IServiceProvider services, InvalidOperationException failure)
        : CodeGeneratorServiceBase(provider, services)
    {
        protected override IEnumerable<CodeData> GenerateCodeWithContext(NodeData node, CodeGenerationContext context)
        {
            yield return new CodeData(context.ApplyIndentedFormat("before-error").ToString(), node);
            throw failure;
        }
    }
    private sealed class NullPoint(CodeGeneratorServiceProvider provider, IServiceProvider services) : CurvePointCodeGeneration(provider, services)
    {
        public override CurvePointOutput Generate(NodeData node, CodeGenerationContext context, string name, bool first) => null!;
    }

    private sealed class CountingDefaults(IServiceProvider services) : DefaultValueService(services)
    {
        public int Calls { get; private set; }

        protected override string GetValueWithDefault(NodeData node, NodeContext context, string key,
            string defaultValue = "") => $"value-{++Calls}";
    }
}
