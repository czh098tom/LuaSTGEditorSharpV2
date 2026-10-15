using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Model;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Tests;

public sealed class CodeGenerationSubServiceTests
{
    [Fact]
    public void RepeatProtocolSupportsHeterogeneousNodesWithOneDirectCall()
    {
        using var fixture = new Fixture();
        var increment = fixture.Repeat("increment", (node, _, times) =>
            new RepeatResult(new CodeFragment([new(node.GetProperty("name") + times, node)]),
                new CodeFragment([new("increment", node)])));
        var linear = fixture.Repeat("linear", (node, _, times) =>
            new RepeatResult(new CodeFragment([new(node.GetProperty("target"), node)]),
                new CodeFragment([new("linear" + times, node)])));
        var first = fixture.Add("increment");
        var second = fixture.Add("linear");
        first.Properties["name"] = "left";
        second.Properties["target"] = "right";
        using var scope = fixture.Context.AcquireContextHandle(fixture.Parent);
        var firstResult = fixture.FindRepeat(first).Generate(first, fixture.Context, "1");
        var secondResult = fixture.FindRepeat(second).Generate(second, fixture.Context, "2");
        Assert.Equal("left1", firstResult.Initialization[0].Content);
        Assert.Equal("linear2", secondResult.Update[0].Content);
        Assert.Equal(1, increment.Calls);
        Assert.Equal(1, linear.Calls);
    }

    [Fact]
    public void CurveProtocolSeparatesTargetAndVariableName()
    {
        using var fixture = new Fixture();
        var service = fixture.Curve("curve", (node, _, target, name) =>
            new CurveResult(new CodeFragment([new("local " + name, node)]),
                new CodeFragment([new("task(" + target + "," + name + ")", node)])));
        var node = fixture.Add("curve");
        using var scope = fixture.Context.AcquireContextHandle(fixture.Parent);
        var result = service.Generate(node, fixture.Context, "self", "_0");
        Assert.Equal("local _0", result.Declaration[0].Content);
        Assert.Equal("task(self,_0)", result.Task[0].Content);
    }

    [Fact]
    public void CurveCoordinatorFrontLoadsDeclarationsAndKeepsOrdinaryOrder()
    {
        using var fixture = new Fixture();
        fixture.Register("ordinary", new MarkerService(fixture.Provider, fixture.Services));
        var service = fixture.Curve("curve", (node, _, target, name) =>
            new CurveResult(new CodeFragment([new("declare " + name, node)]),
                new CodeFragment([new("task " + target + " " + name, node)])));
        var first = fixture.Add("curve");
        var ordinary = fixture.Add("ordinary");
        var second = fixture.Add("curve");
        using var scope = fixture.Context.AcquireContextHandle(fixture.Parent);
        var pairs = fixture.Provider.GetServicesPairForLogicalChildrenOfType<CurveService>(fixture.Parent).ToArray();
        var results = pairs.ToDictionary(pair => pair.NodeData,
            pair => pair.Service.Generate(pair.NodeData, fixture.Context, "self", "_" + Array.IndexOf(pairs, pair)));
        var code = pairs.SelectMany(pair => results[pair.NodeData].Declaration).ToList();
        foreach (var child in fixture.Parent.GetLogicalChildren())
        {
            if (results.TryGetValue(child, out var result)) code.AddRange(result.Task);
            else code.AddRange(fixture.Provider.GenerateCode(child, fixture.Context));
        }
        Assert.Equal(new[] { "declare _0", "declare _1", "task self _0", "marker", "task self _1" },
            code.Select(data => data.Content));
        Assert.Same(ordinary, code[3].Source);
        Assert.Equal(2, service.Calls);
        Assert.Same(first, code[0].Source);
        Assert.Same(second, code[1].Source);
        Assert.Same(fixture.Parent, fixture.Context.Peek());
    }

    [Fact]
    public void InactiveLogicalChildrenAreNotReturnedByProtocolLookup()
    {
        using var fixture = new Fixture();
        fixture.Curve("curve", (_, _, _, _) => new CurveResult(new CodeFragment([]), new CodeFragment([])));
        var active = fixture.Add("curve");
        var inactive = fixture.Add("curve");
        inactive.IsActive = false;
        Assert.Single(fixture.Provider.GetServicesPairForLogicalChildrenOfType<CurveService>(fixture.Parent));
        Assert.True(active.IsActive);
    }

    [Fact]
    public void SubserviceOrdinaryGenerationIsSilentAndCanBeExplicitlyOverridden()
    {
        using var fixture = new Fixture();
        fixture.Repeat("silent", (_, _, _) => new RepeatResult(new CodeFragment([]), new CodeFragment([])));
        fixture.Register("ordinary", new OrdinaryService(fixture.Provider, fixture.Services));
        var silent = fixture.Add("silent");
        silent.Add(new NodeData("marker"));
        var ordinary = fixture.Add("ordinary");
        ordinary.Add(new NodeData("marker"));
        fixture.Register("marker", new MarkerService(fixture.Provider, fixture.Services));
        var generated = fixture.Provider.GenerateCode(fixture.Parent, new LocalServiceParam(null!)).ToArray();
        Assert.Single(generated);
        Assert.Same(ordinary.PhysicalChildren[0], generated[0].Source);
    }

    [Fact]
    public void NestedDirectCallsUseExplicitParametersWithoutContextFrame()
    {
        using var fixture = new Fixture();
        RepeatProbe? inner = null;
        var outer = fixture.Repeat("outer", (node, context, times) =>
        {
            using var scope = context.AcquireContextHandle(node);
            var nested = inner!.Generate(node.PhysicalChildren[0], context, "inner");
            return new RepeatResult(new CodeFragment([new(times + nested.Update[0].Content, node)]),
                new CodeFragment([]));
        });
        inner = fixture.Repeat("inner", (_, _, times) =>
            new RepeatResult(new CodeFragment([]), new CodeFragment([new(times, fixture.Parent)])));
        var outerNode = fixture.Add("outer");
        outerNode.Add(new NodeData("inner"));
        using var parentScope = fixture.Context.AcquireContextHandle(fixture.Parent);
        var result = outer.Generate(outerNode, fixture.Context, "outer");
        Assert.Equal("outerinner", result.Initialization[0].Content);
        Assert.Equal(1, outer.Calls);
        Assert.Equal(1, inner.Calls);
        Assert.Same(fixture.Parent, fixture.Context.Peek());
    }

    [Fact]
    public void DirectCallSeesCompleteAncestorChainIncludingLogicalContainer()
    {
        using var fixture = new Fixture();
        var root = new NodeData("root");
        var container = new NodeData("container");
        var service = fixture.Repeat("child", (node, context, _) =>
        {
            Assert.Equal(new[] { root, container, fixture.Parent }, context.EnumerateFromFarest());
            return new RepeatResult(new CodeFragment([]), new CodeFragment([]));
        });
        root.Add(container);
        container.Add(fixture.Parent);
        var child = fixture.Add("child");
        using var rootScope = fixture.Context.AcquireContextHandle(root);
        using var containerScope = fixture.Context.AcquireContextHandle(container);
        using var parentScope = fixture.Context.AcquireContextHandle(fixture.Parent);
        service.Generate(child, fixture.Context, "1");
    }

    [Fact]
    public void FragmentMaterializesCopiesAndPreservesSourceAndLineCount()
    {
        var source = new NodeData("source");
        var input = new List<CodeData> { new("one\n", source) };
        var fragment = new CodeFragment(input);
        input.Clear();
        Assert.Equal("one\n", fragment[0].Content);
        Assert.Equal(1, fragment[0].LineCount);
        Assert.Same(source, fragment[0].Source);
        Assert.Throws<ArgumentNullException>(() => new CodeFragment(null!));
    }

    [Fact]
    public void MaterializedResultSurvivesLaterNestedGenerationAndKeepsFixedIndention()
    {
        using var fixture = new Fixture();
        var evaluations = 0;
        IEnumerable<CodeData> Render(NodeData node, CodeGenerationContext context, string template)
        {
            evaluations++;
            yield return new CodeData(context.ApplyIndentedFormat(template).ToString(), node);
        }
        var service = fixture.Repeat("repeat", (node, context, _) =>
            new RepeatResult(
                Render(node, context, "init"), Render(node, context, "{0:IND}update")));
        var first = fixture.Add("repeat");
        var nested = new NodeData("repeat");
        first.Add(nested);
        using var parentScope = fixture.Context.AcquireContextHandle(fixture.Parent, 1);
        var saved = service.Generate(first, fixture.Context, "saved");
        Assert.Equal(2, evaluations);
        using (fixture.Context.AcquireContextHandle(first, 2))
        {
            var later = service.Generate(nested, fixture.Context, "later");
            Assert.Equal("    update\n", later.Update[0].Content);
            Assert.Equal(" ", fixture.Context.Format("{0:IND}"));
        }
        Assert.Equal(" init\n", saved.Initialization[0].Content);
        Assert.Equal("  update\n", saved.Update[0].Content);
        Assert.Equal(4, evaluations);
        Assert.Equal(" ", fixture.Context.Format("{0:IND}"));
    }

    [Fact]
    public void PushFailureDoesNotMutateContextAndFinalPopRestoresEmptyScope()
    {
        using var fixture = new Fixture();
        using (fixture.Context.AcquireContextHandle(fixture.Parent, 2))
        {
            Assert.Throws<ArgumentException>(() => fixture.Context.AcquireContextHandle(fixture.Parent, 3));
            Assert.Throws<OverflowException>(() => fixture.Context.AcquireContextHandle(new NodeData("overflow"), int.MaxValue));
            Assert.Equal(2, fixture.Context.IndentionLevel);
        }
        Assert.Null(fixture.Context.Peek());
        Assert.Equal(0, fixture.Context.IndentionLevel);
        Assert.Empty(fixture.Context.ContextVariables);
    }

    [Fact]
    public void EarlyOrdinaryEnumerationDisposalRestoresParentScope()
    {
        using var fixture = new Fixture();
        fixture.Register("marker", new MarkerService(fixture.Provider, fixture.Services));
        var child = fixture.Add("ordinary");
        child.Add(new NodeData("marker"));
        using var parentScope = fixture.Context.AcquireContextHandle(fixture.Parent);
        using (var iterator = fixture.Provider.GenerateForChildren(child, fixture.Context, 2).GetEnumerator())
        {
            Assert.True(iterator.MoveNext());
            Assert.Same(child, fixture.Context.Peek());
        }
        Assert.Same(fixture.Parent, fixture.Context.Peek());
        Assert.Equal(0, fixture.Context.IndentionLevel);
    }

    [Fact]
    public void MaterializationFailureRestoresNestedScope()
    {
        using var fixture = new Fixture();
        IEnumerable<CodeData> Fail(NodeData node)
        {
            yield return new CodeData("partial", node);
            throw new InvalidOperationException("materialization failed");
        }
        var service = fixture.Repeat("outer", (node, context, _) =>
        {
            using var scope = context.AcquireContextHandle(node, 2);
            return new RepeatResult([], Fail(node));
        });
        var child = fixture.Add("outer");
        using var parentScope = fixture.Context.AcquireContextHandle(fixture.Parent, 1);
        var error = Assert.Throws<InvalidOperationException>(() => service.Generate(child, fixture.Context, "1"));
        Assert.Equal("materialization failed", error.Message);
        Assert.Same(fixture.Parent, fixture.Context.Peek());
        Assert.Equal(1, fixture.Context.IndentionLevel);
        Assert.Single(fixture.Context.ContextVariables);
    }

    [Fact]
    public void LanguageResolutionFailureDoesNotMutateContext()
    {
        using var services = new ServiceCollection().AddSingleton<CodeGeneratorServiceProvider>().BuildServiceProvider();
        var context = new CodeGenerationContext(services, new LocalServiceParam(null!), new CodeGenerationServiceSettings());
        Assert.Throws<InvalidOperationException>(() => context.AcquireContextHandle(new NodeData("node"), 3));
        Assert.Null(context.Peek());
        Assert.Equal(0, context.IndentionLevel);
        Assert.Empty(context.ContextVariables);
    }

    [Fact]
    public void PopRestoresEffectiveLanguageAcrossNullFramesAndAfterFinalPop()
    {
        using var fixture = new Fixture();
        fixture.Language("words", @"\b{0}\b");
        fixture.Language("brackets", @"\[{0}\]");
        var macro = new NodeData("macro");
        macro.Properties["replace"] = "old";
        macro.Properties["by"] = "new";
        var outer = new NodeData("words");
        var bridge = new NodeData("bridge");
        var inner = new NodeData("brackets");
        macro.Add(outer);
        outer.Add(bridge);
        bridge.Add(inner);
        using (fixture.Context.AcquireContextHandle(macro))
        {
            using (fixture.Context.AcquireContextHandle(outer))
            using (fixture.Context.AcquireContextHandle(bridge))
            {
                Assert.Equal("[new] new", fixture.Context.ApplyMacro(bridge, "[old] old"));
                using (fixture.Context.AcquireContextHandle(inner))
                {
                    Assert.Equal("new old", fixture.Context.ApplyMacro(inner, "[old] old"));
                }
                Assert.Equal("[new] new", fixture.Context.ApplyMacro(bridge, "[old] old"));
            }
            Assert.Equal("[old] old", fixture.Context.ApplyMacro(macro, "[old] old"));
        }
        using (fixture.Context.AcquireContextHandle(macro))
        {
            Assert.Equal("[old] old", fixture.Context.ApplyMacro(macro, "[old] old"));
        }
        Assert.Null(fixture.Context.Peek());
    }

    private sealed class Fixture : IDisposable
    {
        private static readonly PackageInfo Package = new(new PackageManifest("direct", new Version(1, 0), 0f, null), string.Empty);
        private readonly List<IDisposable> _registrations = [];
        public ServiceProvider Services { get; } = new ServiceCollection()
            .AddSingleton<LanguageProviderService>().AddSingleton<CodeGeneratorServiceProvider>().BuildServiceProvider();
        public CodeGeneratorServiceProvider Provider => Services.GetRequiredService<CodeGeneratorServiceProvider>();
        public NodeData Parent { get; } = new("parent");
        public CodeGenerationContext Context { get; }

        public Fixture()
        {
            Context = new(Services, new LocalServiceParam(null!), new CodeGenerationServiceSettings { IndentionString = " " });
        }

        public NodeData Add(string typeUid)
        {
            var node = new NodeData(typeUid);
            Parent.Add(node);
            return node;
        }

        public RepeatService FindRepeat(NodeData node) => Provider.GetServicesPairForLogicalChildrenOfType<RepeatService>(Parent)
            .Single(pair => ReferenceEquals(pair.NodeData, node)).Service;
        public void Register(string typeUid, CodeGeneratorServiceBase service) => _registrations.Add(Provider.Register(typeUid, Package, service));
        public RepeatProbe Repeat(string typeUid, Func<NodeData, CodeGenerationContext, string, RepeatResult> callback)
        {
            var service = new RepeatProbe(Provider, Services, callback);
            Register(typeUid, service);
            return service;
        }
        public CurveProbe Curve(string typeUid, Func<NodeData, CodeGenerationContext, string, string, CurveResult> callback)
        {
            var service = new CurveProbe(Provider, Services, callback);
            Register(typeUid, service);
            return service;
        }
        public void Dispose()
        {
            foreach (var registration in _registrations.AsEnumerable().Reverse()) registration.Dispose();
            Services.Dispose();
        }
        public void Language(string name, string template)
        {
            _registrations.Add(Services.GetRequiredService<LanguageProviderService>()
                .Register(name, Package, new TestLanguage(Services, name, template)));
            var service = new CodeGeneratorServiceBase(Provider, Services);
            Newtonsoft.Json.JsonConvert.PopulateObject(
                Newtonsoft.Json.JsonConvert.SerializeObject(new { Language = name }), service);
            Register(name, service);
        }
    }

    private sealed class TestLanguage : LanguageBase
    {
        public override string Name { get; }
        public TestLanguage(IServiceProvider services, string name, string template) : base(services)
        {
            Name = name;
            MacroTemplate = template;
        }
    }

    private sealed class RepeatResult(IEnumerable<CodeData> initialization, IEnumerable<CodeData> update)
    {
        public CodeFragment Initialization { get; } = new(initialization);
        public CodeFragment Update { get; } = new(update);
    }

    private sealed class CurveResult(IEnumerable<CodeData> declaration, IEnumerable<CodeData> task)
    {
        public CodeFragment Declaration { get; } = new(declaration);
        public CodeFragment Task { get; } = new(task);
    }

    private abstract class RepeatService : CodeGenerationSubService
    {
        protected RepeatService(CodeGeneratorServiceProvider provider, IServiceProvider services) : base(provider, services) { }
        public abstract RepeatResult Generate(NodeData node, CodeGenerationContext context, string times);
    }

    private sealed class RepeatProbe(CodeGeneratorServiceProvider provider, IServiceProvider services,
        Func<NodeData, CodeGenerationContext, string, RepeatResult> callback) : RepeatService(provider, services)
    {
        public int Calls { get; private set; }
        public override RepeatResult Generate(NodeData node, CodeGenerationContext context, string times)
        {
            Calls++;
            return callback(node, context, times);
        }
    }

    private abstract class CurveService : CodeGenerationSubService
    {
        protected CurveService(CodeGeneratorServiceProvider provider, IServiceProvider services) : base(provider, services) { }
        public abstract CurveResult Generate(NodeData node, CodeGenerationContext context, string target, string variableName);
    }

    private sealed class CurveProbe(CodeGeneratorServiceProvider provider, IServiceProvider services,
        Func<NodeData, CodeGenerationContext, string, string, CurveResult> callback) : CurveService(provider, services)
    {
        public int Calls { get; private set; }
        public override CurveResult Generate(NodeData node, CodeGenerationContext context, string target, string variableName)
        {
            Calls++;
            return callback(node, context, target, variableName);
        }
    }

    private sealed class OrdinaryService(CodeGeneratorServiceProvider provider, IServiceProvider services)
        : CodeGenerationSubService(provider, services)
    {
        protected override IEnumerable<CodeData> GenerateCodeWithContext(NodeData node, CodeGenerationContext context)
            => NodeServiceProvider.GenerateForChildren(node, context, 1);
    }

    private sealed class MarkerService(CodeGeneratorServiceProvider provider, IServiceProvider services)
        : CodeGeneratorServiceBase(provider, services)
    {
        protected override IEnumerable<CodeData> GenerateCodeWithContext(NodeData node, CodeGenerationContext context)
        {
            yield return new CodeData("marker", node);
        }
    }
}
