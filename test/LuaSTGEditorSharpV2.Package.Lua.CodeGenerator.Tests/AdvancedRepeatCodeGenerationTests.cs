using System.Reflection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Package.Lua.CodeGenerator.Specialized.AdvancedRepeat;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.Lua.CodeGenerator.Tests;

public sealed class AdvancedRepeatCodeGenerationTests
{
    [Fact]
    public void AssetsLoadConcreteServicesAndDefaults()
    {
        using var fixture = new Fixture();
        var repeat = Node("AdvancedRepeat");
        var increment = Node("IncrementVariable");
        var linear = Node("LinearVariable");

        Assert.IsType<AdvancedRepeatCodeGeneration>(fixture.Service(repeat));
        Assert.IsType<VariableCollectionCodeGeneration>(fixture.Service(Node("VariableCollection")));
        Assert.IsType<TemplateVariableTransformationCodeGeneration>(fixture.Service(increment));
        Assert.IsType<TemplateVariableTransformationCodeGeneration>(fixture.Service(linear));
        Assert.Equal("_infinite", fixture.Default(repeat, "times"));
        Assert.Equal("", fixture.Default(increment, "name"));
        Assert.Equal("0", fixture.Default(increment, "init"));
        Assert.Equal("0", fixture.Default(increment, "increment"));
        Assert.Equal("", fixture.Default(linear, "name"));
        Assert.Equal("0", fixture.Default(linear, "from"));
        Assert.Equal("0", fixture.Default(linear, "to"));
        Assert.Equal("false", fixture.Default(linear, "precisely"));
        Assert.Equal("MOVE_NORMAL", fixture.Default(linear, "mode"));
        linear.Properties["name"] = "position";
        var variables = Node("VariableCollection");
        variables.Add(linear);
        repeat.Add(variables);
        var generated = string.Concat(fixture.Generate(repeat).Select(item => item.Content));
        Assert.Contains("for _=1,_infinite do", generated);
        Assert.Contains("/(_infinite)", generated);
    }

    [Fact]
    public void MixedVariablesEmitV1OrderAndSources()
    {
        using var fixture = new Fixture();
        var repeat = Repeat("4");
        var increment = Node("IncrementVariable", ("name", "counter"), ("init", "2"), ("increment", "3"));
        var linear = Node("LinearVariable", ("name", "position"), ("from", "10"), ("to", "20"));
        var marker = Node("marker");
        repeat.PhysicalChildren[0].Add(increment);
        repeat.PhysicalChildren[0].Add(linear);
        repeat.Add(marker);

        var code = fixture.Generate(repeat);
        Assert.Equal(new[]
        {
            "do\n",
            " local counter=2 local _d_counter=(3)\n",
            " local _beg_position=10 local position=_beg_position local _end_position=20 local _d_position=(_end_position-_beg_position)/(4)\n",
            " for _=1,4 do\n",
            "  marker\n",
            "  counter=counter+_d_counter\n",
            "  position=position+_d_position\n",
            " end\n",
            "end\n"
        }, code.Select(item => item.Content));
        Assert.Same(increment, code[1].Source);
        Assert.Same(linear, code[2].Source);
        Assert.Same(marker, code[4].Source);
        Assert.Same(increment, code[5].Source);
        Assert.Same(linear, code[6].Source);
        Assert.All(code, item => Assert.Equal(1, item.LineCount));
    }

    [Theory]
    [InlineData("MOVE_NORMAL", "false", "3", "local _d_position=(_end_position-_beg_position)/(3)", "position=position+_d_position")]
    [InlineData("MOVE_NORMAL", "true", "3", "local _d_position=(_end_position-_beg_position)/(3-1)", "position=position+_d_position")]
    [InlineData("MOVE_ACCEL", "false", "3", "local _w_position=0 local _d_w_position=1/(3)", "_w_position=_w_position+_d_w_position position=(_end_position-_beg_position)*_w_position^2+_beg_position")]
    [InlineData("MOVE_ACCEL", "true", "3", "local _w_position=0 local _d_w_position=1/(3-1)", "_w_position=_w_position+_d_w_position position=(_end_position-_beg_position)*_w_position^2+_beg_position")]
    [InlineData("MOVE_DECEL", "false", "3", "local _w_position=0 local _d_w_position=1/(3)", "_w_position=_w_position+_d_w_position position=(_beg_position-_end_position)*(_w_position-1)^2+_end_position")]
    [InlineData("MOVE_DECEL", "true", "3", "local _w_position=0 local _d_w_position=1/(3-1)", "_w_position=_w_position+_d_w_position position=(_beg_position-_end_position)*(_w_position-1)^2+_end_position")]
    [InlineData("MOVE_ACC_DEC", "false", "3", "local _w_position=0 local _d_w_position=1/(3)", "_w_position=_w_position+_d_w_position if _w_position<0.5 then position=2*(_end_position-_beg_position)*_w_position^2+_beg_position else position=(_end_position-_beg_position)*(-2*_w_position^2+4*_w_position-1)+_beg_position end")]
    [InlineData("MOVE_ACC_DEC", "true", "3", "local _w_position=0 local _d_w_position=1/(3-1)", "_w_position=_w_position+_d_w_position if _w_position<0.5 then position=2*(_end_position-_beg_position)*_w_position^2+_beg_position else position=(_end_position-_beg_position)*(-2*_w_position^2+4*_w_position-1)+_beg_position end")]
    [InlineData("unknown'quote", "false", "3", "local _d_position=(_end_position-_beg_position)/(3)", "position=position+_d_position")]
    [InlineData("MOVE_NORMAL", "true", "1", "local _d_position=(_end_position-_beg_position)/(1-1)", "position=position+_d_position")]
    public void LinearModesPreserveV1(string mode, string precisely, string times, string initialization, string update)
    {
        using var fixture = new Fixture();
        var repeat = Repeat(times);
        var variable = Node("LinearVariable", ("name", "position"), ("from", "1"), ("to", "9"),
            ("mode", mode), ("precisely", precisely));
        repeat.PhysicalChildren[0].Add(variable);

        var fragments = fixture.Generate(repeat).Where(item => ReferenceEquals(item.Source, variable)).ToArray();
        Assert.Equal(2, fragments.Length);
        Assert.Equal(" local _beg_position=1 local position=_beg_position local _end_position=9 " + initialization + "\n",
            fragments[0].Content);
        Assert.Equal("  " + update + "\n", fragments[1].Content);
    }

    [Fact]
    public void DisabledVariablesAreSkippedAndCollectionIsSilent()
    {
        using var fixture = new Fixture();
        var repeat = Repeat("2");
        var disabled = Node("IncrementVariable", ("name", "ignored"));
        disabled.IsActive = false;
        repeat.PhysicalChildren[0].Add(disabled);
        Assert.Equal(new[] { "do\n", " for _=1,2 do\n", " end\n", "end\n" },
            fixture.Generate(repeat).Select(item => item.Content));
        Assert.Empty(fixture.Generate(repeat.PhysicalChildren[0]));
    }

    [Fact]
    public void NestedRepeatsKeepDistinctTimesAndCaptures()
    {
        using var fixture = new Fixture();
        var outer = Repeat("2");
        var inner = Repeat("5");
        outer.PhysicalChildren[0].Add(Node("LinearVariable", ("name", "outer")));
        inner.PhysicalChildren[0].Add(Node("LinearVariable", ("name", "inner")));
        outer.Add(inner);

        var code = string.Concat(fixture.Generate(outer).Select(item => item.Content));
        Assert.Contains(" local _beg_outer=0 local outer=_beg_outer local _end_outer=0 local _d_outer=(_end_outer-_beg_outer)/(2)\n", code);
        Assert.Contains("   local _beg_inner=0 local inner=_beg_inner local _end_inner=0 local _d_inner=(_end_inner-_beg_inner)/(5)\n", code);
        Assert.Contains(" for _=1,2 do\n", code);
        Assert.Contains("   for _=1,5 do\n", code);
        Assert.Contains("    inner=inner+_d_inner\n", code);
        Assert.Contains("  outer=outer+_d_outer\n", code);
        Assert.Null(fixture.Context.Peek());
    }

    [Fact]
    public void MultipleNonEmptyCollectionsMaterializeInOrderAndKeepScopesIsolated()
    {
        using var fixture = new Fixture();
        var firstProbe = new Probe(fixture.Generator, fixture.Services);
        var secondProbe = new Probe(fixture.Generator, fixture.Services);
        fixture.Register("first-custom", firstProbe);
        fixture.Register("second-custom", secondProbe);
        var repeat = Node("AdvancedRepeat", ("times", "7"));
        var firstCollection = Node("VariableCollection");
        var secondCollection = Node("VariableCollection");
        var firstVariable = Node("first-custom");
        var secondVariable = Node("second-custom");
        firstCollection.Add(firstVariable);
        secondCollection.Add(secondVariable);
        repeat.Add(firstCollection);
        repeat.Add(secondCollection);
        repeat.Add(Node("marker"));

        var code = fixture.Generate(repeat);
        Assert.Equal(1, firstProbe.Calls);
        Assert.Equal(1, secondProbe.Calls);
        Assert.Same(repeat, firstProbe.CapturedRepeat);
        Assert.Same(repeat, secondProbe.CapturedRepeat);
        Assert.Equal(new[] { repeat, firstCollection }, firstProbe.Ancestors);
        Assert.Equal(new[] { repeat, secondCollection }, secondProbe.Ancestors);
        Assert.Equal(new[] { "do\n", " probe-init\n", " probe-init\n", " for _=1,7 do\n", "  marker\n", "  probe-update\n", "  probe-update\n", " end\n", "end\n" },
            code.Select(item => item.Content));
        Assert.Same(firstVariable, code[1].Source);
        Assert.Same(secondVariable, code[2].Source);
        Assert.Same(firstVariable, code[5].Source);
        Assert.Same(secondVariable, code[6].Source);
        Assert.Null(fixture.Context.Peek());
    }

    [Fact]
    public void MacrosExpandEachRawCaptureWithoutReexpandingResults()
    {
        using var fixture = new Fixture();
        var macro = Node("macro", ("replace", "COUNT"), ("by", "COUNT+1"));
        var repeat = Repeat("COUNT");
        repeat.PhysicalChildren[0].Add(Node("IncrementVariable",
            ("name", "COUNT"), ("init", "COUNT"), ("increment", "COUNT")));
        repeat.PhysicalChildren[0].Add(Node("LinearVariable", ("name", "position")));
        macro.Add(repeat);

        var code = string.Concat(fixture.Generate(macro).Select(item => item.Content));
        Assert.Contains("for _=1,COUNT+1 do", code);
        Assert.Contains("local COUNT=COUNT+1 local _d_COUNT=(COUNT+1)", code);
        Assert.Contains("(_end_position-_beg_position)/(COUNT+1)", code);
        Assert.DoesNotContain("COUNT+1+1", code);
        Assert.Null(fixture.Context.Peek());
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("multiple")]
    [InlineData("incompatible")]
    [InlineData("disabled")]
    public void ZeroOrManyCollectionsAreLegalAndIncompatibleChildrenKeepTheirSource(string kind)
    {
        using var fixture = new Fixture();
        var repeat = Node("AdvancedRepeat");
        NodeData source = repeat;
        if (kind != "missing")
        {
            var collection = Node("VariableCollection");
            repeat.Add(collection);
            if (kind == "multiple")
            {
                repeat.Add(Node("VariableCollection"));
            }
            if (kind == "incompatible")
            {
                source = Node("ordinary");
                collection.Add(source);
            }
            collection.IsActive = kind != "disabled";
        }

        if (kind == "incompatible")
        {
            var error = Assert.Throws<AdvancedRepeatGenerationException>(() => fixture.Generate(repeat));
            Assert.Same(source, error.SourceNode);
        }
        else
        {
            Assert.Equal(new[] { "do\n", " for _=1,_infinite do\n", " end\n", "end\n" },
                fixture.Generate(repeat).Select(item => item.Content));
        }
        Assert.Null(fixture.Context.Peek());
        Assert.Equal(0, fixture.Context.IndentionLevel);
    }

    [Fact]
    public void ParentAndAncestorCapturesAndAllPhaseTemplatesAreConfigurable()
    {
        using var fixture = new Fixture();
        var repeat = Repeat("999");
        repeat.PhysicalChildren[0].Add(Node("LinearVariable", ("name", "position")));
        var parent = (AdvancedRepeatCodeGeneration)fixture.Service(repeat);
        parent.Captures = JsonConvert.DeserializeObject<Dictionary<string, CaptureWithMacroOption>>("""
            { "limit": { "Key": "iterations", "DefaultValue": "6", "DisableMacro": true } }
            """)!;
        parent.Head = ["do -- configured"];
        parent.LoopHead = ["{0:IND}for configured=1,{{{self.limit}}} do"];
        parent.LoopTail = ["{0:IND}end -- loop"];
        parent.Tail = ["end -- scope"];
        var variable = (TemplateVariableTransformationCodeGeneration)fixture.Service(Node("LinearVariable"));
        variable.ContextCaptures["repeat"] = JsonConvert.DeserializeObject<ContextCapture>("""
            {
                "TypeUID": "AdvancedRepeat",
                "Property": { "times": { "Key": "iterations", "DefaultValue": "6", "DisableMacro": true } }
            }
            """)!;

        var saved = fixture.Generate(repeat);
        repeat.Properties["iterations"] = "8";
        var changed = fixture.Generate(repeat);

        Assert.Equal(new[] { "do -- configured\n", " for configured=1,6 do\n", " end -- loop\n", "end -- scope\n" },
            saved.Where(item => ReferenceEquals(item.Source, repeat)).Select(item => item.Content));
        Assert.Contains("/(6)\n", string.Concat(saved.Select(item => item.Content)));
        Assert.Contains(" for configured=1,8 do\n", changed.Select(item => item.Content));
        Assert.Contains("/(8)\n", string.Concat(changed.Select(item => item.Content)));
        Assert.All(saved, item => Assert.Equal(1, item.LineCount));
    }

    [Fact]
    public void ParentAndAncestorMacroExpansionCanBeDisabledByConfiguration()
    {
        using var fixture = new Fixture();
        var macro = Node("macro", ("replace", "COUNT"), ("by", "COUNT+1"));
        var repeat = Repeat("COUNT");
        repeat.PhysicalChildren[0].Add(Node("LinearVariable", ("name", "position")));
        macro.Add(repeat);
        var parent = (AdvancedRepeatCodeGeneration)fixture.Service(repeat);
        parent.Captures["times"].DisableMacro = true;
        var variable = (TemplateVariableTransformationCodeGeneration)fixture.Service(Node("LinearVariable"));
        variable.ContextCaptures["repeat"].Property["times"].DisableMacro = true;

        var code = string.Concat(fixture.Generate(macro).Select(item => item.Content));

        Assert.Contains("for _=1,COUNT do", code);
        Assert.Contains("(_end_position-_beg_position)/(COUNT)", code);
        Assert.DoesNotContain("COUNT+1", code);
    }

    [Theory]
    [InlineData("Head", false)]
    [InlineData("Head", true)]
    [InlineData("LoopHead", false)]
    [InlineData("LoopHead", true)]
    [InlineData("LoopTail", false)]
    [InlineData("LoopTail", true)]
    [InlineData("Tail", false)]
    [InlineData("Tail", true)]
    public void ParentPhaseTemplatesAreRequiredAndNonNull(string member, bool explicitNull)
    {
        using var fixture = new Fixture();
        var parent = (AdvancedRepeatCodeGeneration)fixture.Service(Node("AdvancedRepeat"));
        var config = Newtonsoft.Json.Linq.JObject.Parse("""
            {
                "Head": [],
                "LoopHead": [],
                "LoopTail": [],
                "Tail": []
            }
            """);
        if (explicitNull)
        {
            config[member] = Newtonsoft.Json.Linq.JValue.CreateNull();
        }
        else
        {
            config.Remove(member);
        }

        Assert.Throws<JsonSerializationException>(() => JsonConvert.PopulateObject(config.ToString(), parent));
    }

    [Fact]
    public void EmptyParentTemplatesPreserveBodyAndRuntimeNullReportsRepeat()
    {
        using var fixture = new Fixture();
        var repeat = Repeat("1");
        var marker = Node("marker");
        repeat.Add(marker);
        var parent = (AdvancedRepeatCodeGeneration)fixture.Service(repeat);
        parent.Head = [];
        parent.LoopHead = [];
        parent.LoopTail = [];
        parent.Tail = [];

        var code = fixture.Generate(repeat);
        Assert.Single(code);
        Assert.Equal("  marker\n", code[0].Content);
        Assert.Same(marker, code[0].Source);

        parent.Head = null!;
        var error = Assert.Throws<AdvancedRepeatGenerationException>(() => fixture.Generate(repeat));
        Assert.Same(repeat, error.SourceNode);
        Assert.IsType<ArgumentNullException>(error.InnerException);
        Assert.Null(fixture.Context.Peek());
    }

    [Fact]
    public void TemplatesDistinguishEmptyMissingAndLiteralValues()
    {
        using var fixture = new Fixture();
        var parent = Node("parent");
        var node = Node("template");
        parent.Add(node);
        using var scope = fixture.Context.AcquireContextHandle(parent, 1);
        var arguments = new Dictionary<string, object?> { ["literal"] = "{0:IND}<>&" };
        var model = CodeFragmentTemplate.CreateModel(fixture.Services, node, fixture.Context,
            new Dictionary<string, CaptureWithMacroOption>(), new Dictionary<string, ContextCapture>(),
            new Dictionary<string, string>(), arguments);
        var fragment = CodeFragmentTemplate.Render(["{0:IND}local value={ text = '{{{args.literal}}}' }"], model, node, fixture.Context);
        Assert.Equal("  local value={ text = '{0:IND}<>&' }\n", fragment[0].Content);
        Assert.Same(node, fragment[0].Source);
        Assert.Empty(CodeFragmentTemplate.Render([], model, node, fixture.Context));
        Assert.Throws<ArgumentNullException>(() => CodeFragmentTemplate.Render(null!, model, node, fixture.Context));

        var variable = (TemplateVariableTransformationCodeGeneration)fixture.Service(Node("IncrementVariable"));
        Assert.Throws<JsonSerializationException>(() => JsonConvert.PopulateObject("{\"Initialization\":null,\"Update\":[]}", variable));
        Assert.Throws<JsonSerializationException>(() => JsonConvert.PopulateObject("{\"Captures\":null,\"Initialization\":[],\"Update\":[]}", variable));
    }

    [Fact]
    public void TemplateArrayConcatenatesBeforeMustacheAndKeepsLineCount()
    {
        using var fixture = new Fixture();
        var parent = Node("parent");
        var node = Node("template");
        parent.Add(node);
        using var scope = fixture.Context.AcquireContextHandle(parent, 1);
        var model = CodeFragmentTemplate.CreateModel(fixture.Services, node, fixture.Context,
            new Dictionary<string, CaptureWithMacroOption>(), new Dictionary<string, ContextCapture>(),
            new Dictionary<string, string> { ["ready"] = "'{{args.enabled}}' == 'true'" },
            new Dictionary<string, object?> { ["enabled"] = "true" });
        var fragment = CodeFragmentTemplate.Render(["fir", "st\n{{#condition.", "ready}}sec", "ond{{/condition.ready}}"],
            model, node, fixture.Context);

        Assert.Equal(" first\n second\n", fragment[0].Content);
        Assert.Equal(2, fragment[0].LineCount);
        Assert.Same(node, fragment[0].Source);
    }

    [Fact]
    public void ConditionsReadSelfAncestorArgsAndEarlierConditionsInOrder()
    {
        using var fixture = new Fixture();
        var parent = Node("holder", ("prefix", "root"));
        var node = Node("template", ("value", "child"));
        parent.Add(node);
        using var scope = fixture.Context.AcquireContextHandle(parent);
        var captures = JsonConvert.DeserializeObject<Dictionary<string, CaptureWithMacroOption>>(
            """{ "value": { "Key": "value", "DisableMacro": true } }""")!;
        var contextCaptures = JsonConvert.DeserializeObject<Dictionary<string, ContextCapture>>("""
            { "owner": { "TypeUID": "holder", "Property": { "prefix": { "Key": "prefix", "DisableMacro": true } } } }
            """)!;
        var conditionTemplates = new Dictionary<string, string>
        {
            ["ancestor"] = "'{{context.owner.prefix}}' == 'root'",
            ["argument"] = "'{{args.enabled}}' == 'true'",
            ["self"] = "'{{self.value}}' == 'child'"
        };
        conditionTemplates.Add("all",
            "({{#condition.ancestor}}true{{/condition.ancestor}}{{^condition.ancestor}}false{{/condition.ancestor}})" +
            " && ({{#condition.argument}}true{{/condition.argument}}{{^condition.argument}}false{{/condition.argument}})" +
            " && ({{#condition.self}}true{{/condition.self}}{{^condition.self}}false{{/condition.self}})");
        var arguments = new Dictionary<string, object?> { ["enabled"] = "true" };

        var saved = CodeFragmentTemplate.Render(["{{#condition.all}}ok{{/condition.all}}{{^condition.all}}bad{{/condition.all}}"],
            CodeFragmentTemplate.CreateModel(fixture.Services, node, fixture.Context,
                captures, contextCaptures, conditionTemplates, arguments), node, fixture.Context);
        arguments["enabled"] = "false";
        var changed = CodeFragmentTemplate.Render(["{{#condition.all}}ok{{/condition.all}}{{^condition.all}}bad{{/condition.all}}"],
            CodeFragmentTemplate.CreateModel(fixture.Services, node, fixture.Context,
                captures, contextCaptures, conditionTemplates, arguments), node, fixture.Context);

        Assert.Equal("ok\n", saved[0].Content);
        Assert.Equal("bad\n", changed[0].Content);
    }

    [Fact]
    public void TemplateExpressionsKeepHtmlCharactersAndNamesUseDoubleBraces()
    {
        using var fixture = new Fixture();
        var node = Node("template", ("name", "safe_name"));
        var captures = JsonConvert.DeserializeObject<Dictionary<string, CaptureWithMacroOption>>(
            """{ "name": { "Key": "name", "DisableMacro": true } }""")!;
        var model = CodeFragmentTemplate.CreateModel(fixture.Services, node, fixture.Context, captures,
            new Dictionary<string, ContextCapture>(), new Dictionary<string, string>(),
            new Dictionary<string, object?> { ["expression"] = "(left < right and 'a&b' or \"c>d\")" });
        var fragment = CodeFragmentTemplate.Render(["local {{self.name}}={{{args.expression}}}"], model, node, fixture.Context);

        Assert.Equal("local safe_name=(left < right and 'a&b' or \"c>d\")\n", fragment[0].Content);
    }

    [Fact]
    public void ModelsCaptureOncePerGenerateAndDoNotLeakAcrossGenerates()
    {
        using var fixture = new Fixture();
        var transformation = new TemplateVariableTransformationCodeGeneration(fixture.Generator, fixture.Services)
        {
            Captures = JsonConvert.DeserializeObject<Dictionary<string, CaptureWithMacroOption>>(
                """{ "value": { "Key": "value", "DisableMacro": true } }""")!,
            Initialization = ["{{{self.value}}}"],
            Update = ["{0:IND}{{{self.value}}}"]
        };
        var defaults = new CountingDefaultValueService(fixture.Services);
        fixture.Register("counted", transformation);
        fixture.RegisterDefaults("counted", defaults);
        var repeat = Repeat("2");
        repeat.PhysicalChildren[0].Add(Node("counted"));

        var first = fixture.Generate(repeat);
        var second = fixture.Generate(repeat);

        Assert.Equal(2, defaults.Calls);
        Assert.Contains(" value-1\n", first.Select(item => item.Content));
        Assert.Contains("  value-1\n", first.Select(item => item.Content));
        Assert.Contains(" value-2\n", second.Select(item => item.Content));
        Assert.Contains("  value-2\n", second.Select(item => item.Content));
    }

    [Fact]
    public void PrecollectionExceptionRestoresContextAndKeepsVariableSource()
    {
        using var fixture = new Fixture();
        fixture.Register("throwing", new Probe(fixture.Generator, fixture.Services, true));
        var parent = Node("parent");
        var repeat = Repeat("1");
        var variable = Node("throwing");
        parent.Add(repeat);
        repeat.PhysicalChildren[0].Add(variable);
        using var scope = fixture.Context.AcquireContextHandle(parent, 1);

        var error = Assert.Throws<AdvancedRepeatGenerationException>(() => fixture.Generate(repeat));
        Assert.Same(variable, error.SourceNode);
        Assert.IsType<InvalidOperationException>(error.InnerException);
        Assert.Same(parent, fixture.Context.Peek());
        Assert.Equal(1, fixture.Context.IndentionLevel);
        Assert.Single(fixture.Context.ContextVariables);
    }

    [Fact]
    public void EarlyDisposalDuringBodyRestoresContext()
    {
        using var fixture = new Fixture();
        var parent = Node("parent");
        var repeat = Repeat("1");
        parent.Add(repeat);
        repeat.Add(Node("marker"));
        using var scope = fixture.Context.AcquireContextHandle(parent, 3);

        using (var iterator = fixture.Generator.GenerateCode(repeat, fixture.Context).GetEnumerator())
        {
            Assert.True(iterator.MoveNext());
            Assert.True(iterator.MoveNext());
            Assert.True(iterator.MoveNext());
            Assert.Equal("     marker\n", iterator.Current.Content);
            Assert.Same(repeat, fixture.Context.Peek());
            Assert.Equal(5, fixture.Context.IndentionLevel);
        }
        Assert.Same(parent, fixture.Context.Peek());
        Assert.Equal(3, fixture.Context.IndentionLevel);
        Assert.Single(fixture.Context.ContextVariables);
    }

    private static NodeData Node(string typeUid, params (string Key, string Value)[] properties)
    {
        var node = new NodeData(typeUid);
        foreach (var (key, value) in properties)
        {
            node.Properties[key] = value;
        }
        return node;
    }

    private static NodeData Repeat(string times)
    {
        var repeat = Node("AdvancedRepeat", ("times", times));
        repeat.Add(Node("VariableCollection"));
        return repeat;
    }

    private sealed class Fixture : IDisposable
    {
        private static readonly PackageManifest Manifest = new("Lua", new Version(1, 0), 0f, null);
        private readonly List<IDisposable> _registrations = [];
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
            services.Add(typeof(CodeGeneratorServiceBase).GetCustomAttribute<InjectAttribute>()!.ToDescriptor(typeof(CodeGeneratorServiceBase)));
            services.Add(typeof(MustacheCodeGeneration).GetCustomAttribute<InjectAttribute>()!.ToDescriptor(typeof(MustacheCodeGeneration)));
            foreach (var type in typeof(AdvancedRepeatCodeGeneration).Assembly.GetTypes()
                .Where(type => !type.IsAbstract && typeof(CodeGeneratorServiceBase).IsAssignableFrom(type)))
            {
                var attribute = type.GetCustomAttribute<InjectAttribute>();
                Assert.NotNull(attribute);
                services.Add(attribute.ToDescriptor(type));
            }
            Services = services.BuildServiceProvider();
            Context = new(Services, new LocalServiceParam(null!), new CodeGenerationServiceSettings { IndentionString = " " });
            _registrations.Add(Services.GetRequiredService<LanguageProviderService>()
                .Register("Lua", new PackageInfo(Manifest, ""), new TestLanguage(Services)));
            Register("marker", new Marker(Generator, Services));

            var packed = new PackedServiceCollection();
            packed.Add<CodeGeneratorServiceProvider>();
            packed.Add<DefaultValueServiceProvider>();
            var loader = new NodePackageProvider(NullLogger<NodePackageProvider>.Instance, packed, Services);
            _registrations.Add(loader.LoadPackageFromDirectory(new PackageAssemblyDescriptor(
                "Lua", Path.Combine(AppContext.BaseDirectory, "package", "Lua"),
                Manifest, [typeof(AdvancedRepeatCodeGeneration).Assembly])));
        }

        public CodeGeneratorServiceBase Service(NodeData node)
            => Generator.GetRegisteredAvailableData()[node.TypeUID].data;

        public string Default(NodeData node, string key)
            => new NodePropertyAccessToken(Services, node, Context).GetValueWithDefault(key);

        public CodeData[] Generate(NodeData node) => Generator.GenerateCode(node, Context).ToArray();

        public void Register(string typeUid, CodeGeneratorServiceBase service)
            => _registrations.Add(Generator.Register(typeUid, new PackageInfo(Manifest, ""), service));

        public void RegisterDefaults(string typeUid, DefaultValueService service)
            => _registrations.Add(Services.GetRequiredService<DefaultValueServiceProvider>()
                .Register(typeUid, new PackageInfo(Manifest, ""), service));

        public void Dispose()
        {
            foreach (var registration in _registrations.AsEnumerable().Reverse())
            {
                registration.Dispose();
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

    private sealed class Marker(CodeGeneratorServiceProvider provider, IServiceProvider services)
        : CodeGeneratorServiceBase(provider, services)
    {
        protected override IEnumerable<CodeData> GenerateCodeWithContext(NodeData node, CodeGenerationContext context)
            => [new CodeData(context.ApplyIndentedFormat("marker").ToString(), node)];
    }

    private sealed class Probe(CodeGeneratorServiceProvider provider, IServiceProvider services, bool throws = false)
        : VariableTransformationCodeGeneration(provider, services)
    {
        public int Calls { get; private set; }
        public NodeData? CapturedRepeat { get; private set; }
        public NodeData[] Ancestors { get; private set; } = [];

        public override VariableTransformationOutput Generate(NodeData node, CodeGenerationContext context)
        {
            Calls++;
            CapturedRepeat = context.PeekType("AdvancedRepeat");
            Ancestors = context.EnumerateFromFarest().ToArray();
            if (throws)
            {
                throw new InvalidOperationException("expected");
            }
            return new VariableTransformationOutput(
                [new CodeData(context.ApplyIndentedFormat("probe-init").ToString(), node)],
                [new CodeData(context.ApplyIndentedFormat("{0:IND}probe-update").ToString(), node)]);
        }
    }

    private sealed class CountingDefaultValueService(IServiceProvider serviceProvider) : DefaultValueService(serviceProvider)
    {
        public int Calls { get; private set; }

        protected override string GetValueWithDefault(NodeData dataSource, NodeContext context, string key,
            string defaultValue = "") => $"value-{++Calls}";
    }
}
