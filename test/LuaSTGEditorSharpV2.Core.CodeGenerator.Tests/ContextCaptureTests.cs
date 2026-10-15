using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable;
using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache;
using LuaSTGEditorSharpV2.Core.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using ArrayCapture = LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.ContextCapture;
using NamedCapture = LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache.ContextCapture;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Tests;

public sealed class ContextCaptureTests
{
    [Fact]
    public void SelectorsAreExclusiveAndRoundTripWithoutTheUnusedField()
    {
        foreach (var named in new[] { false, true })
        {
            var type = named ? typeof(NamedCapture) : typeof(ArrayCapture);
            var property = named ? "{}" : "[]";
            foreach (var selector in new[] { "", "\"TypeUID\":\"A\",\"AnyOfTypeUIDs\":[\"B\"],",
                "\"TypeUID\":\"\",", "\"AnyOfTypeUIDs\":[],", "\"AnyOfTypeUIDs\":[\" \"],",
                "\"AnyOfTypeUIDs\":[null],", "\"TypeUID\":null,\"AnyOfTypeUIDs\":[\"A\"],",
                "\"TypeUID\":\"A\",\"AnyOfTypeUIDs\":null," })
                Assert.ThrowsAny<JsonException>(() => JsonConvert.DeserializeObject("{" + selector + "\"Property\":" + property + "}", type));
            foreach (var field in new[] { "TypeUID", "AnyOfTypeUIDs" })
            {
                var value = field == "TypeUID" ? "\"A\"" : "[\"A\",\"B\"]";
                var capture = JsonConvert.DeserializeObject("{\"" + field + "\":" + value + ",\"Property\":" + property + "}", type)!;
                var serialized = JObject.Parse(JsonConvert.SerializeObject(capture));
                Assert.NotNull(serialized[field]);
                Assert.Null(serialized[field == "TypeUID" ? "AnyOfTypeUIDs" : "TypeUID"]);
                Assert.NotNull(JsonConvert.DeserializeObject(serialized.ToString(), type));
                var conflicting = field == "TypeUID" ? "{\"AnyOfTypeUIDs\":[\"B\"]}" : "{\"TypeUID\":\"B\"}";
                var error = Record.Exception(() => JsonConvert.PopulateObject(conflicting, capture));
                Assert.IsType<JsonSerializationException>(error?.GetBaseException());
            }
        }
    }

    [Fact]
    public void AllConsumersSelectNearestMatchAndRestoreOuterScope()
    {
        using var services = new ServiceCollection().AddSingleton<CodeGeneratorServiceProvider>()
            .AddSingleton<LanguageProviderService>().AddSingleton<DefaultValueServiceProvider>().BuildServiceProvider();
        var provider = services.GetRequiredService<CodeGeneratorServiceProvider>();
        var context = new CodeGenerationContext(services, new LocalServiceParam(null!), new CodeGenerationServiceSettings());
        var outer = new NodeData("A"); outer.Properties["name"] = "outer";
        var inner = new NodeData("B"); inner.Properties["name"] = "inner";
        var target = new NodeData("capture");
        var package = new PackageInfo(new PackageManifest("test", new Version(1, 0), 0f, null), string.Empty);
        foreach (var kind in new[] { "plain", "selective", "mustache", "fragment" })
        foreach (var selector in new[] { "\"TypeUID\":\"A\"", "\"AnyOfTypeUIDs\":[\"A\",\"B\"]", "\"AnyOfTypeUIDs\":[\"B\",\"A\"]", "\"AnyOfTypeUIDs\":[\"absent\"]" })
        {
            var named = kind is "mustache" or "fragment";
            var capture = "{" + selector + ",\"Property\":" + (named ? "{\"name\":{\"Key\":\"name\",\"DisableMacro\":true}}" : "[{\"Key\":\"name\",\"DisableMacro\":true}]") + "}";
            CodeGeneratorServiceBase generator = kind switch
            {
                "plain" => new ConfigurableCodeGeneration(provider, services),
                "selective" => new SelectiveCodeGeneration(provider, services),
                _ => new MustacheCodeGeneration(provider, services)
            };
            var head = kind switch { "plain" => "\"{1}\"", "selective" => "[{\"Text\":\"{1}\"}]", _ => "[\"{{{context.owner.name}}}\"]" };
            JsonConvert.PopulateObject("{\"IgnoreChildren\":true,\"Head\":" + head + ",\"ContextCaptures\":" + (named ? "{\"owner\":" + capture + "}" : "[" + capture + "]") + "}", generator);
            using var registration = provider.Register("capture", package, generator);
            string Render()
            {
                if (kind != "fragment") return string.Concat(provider.GenerateCode(target, context).Select(x => x.Content)).TrimEnd('\r', '\n');
                var model = CodeFragmentTemplate.CreateModel(services, target, context, new Dictionary<string, CaptureWithMacroOption>(),
                    new Dictionary<string, NamedCapture> { ["owner"] = JsonConvert.DeserializeObject<NamedCapture>(capture)! }, new Dictionary<string, string>());
                return CodeFragmentTemplate.RenderText(["{{{context.owner.name}}}"], model, context);
            }
            var missing = selector.Contains("absent");
            Assert.Equal("", Render());
            using (context.AcquireContextHandle(outer))
            {
                using (context.AcquireContextHandle(inner))
                using (context.AcquireContextHandle(new NodeData("Folder")))
                    Assert.Equal(missing ? "" : selector.Contains("AnyOfTypeUIDs") ? "inner" : "outer", Render());
                Assert.Equal(missing ? "" : "outer", Render());
            }
            Assert.Equal("", Render());
        }
    }
}
