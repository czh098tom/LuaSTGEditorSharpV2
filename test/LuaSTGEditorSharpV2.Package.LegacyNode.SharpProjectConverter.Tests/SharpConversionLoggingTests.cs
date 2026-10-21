using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Tests;

public sealed class SharpConversionLoggingTests
{
    [Fact]
    public void IfReorderingReportsStructuredSourceLocationToLogger()
    {
        // The first CMEX18 normalization: lines 466, 467, 469, 470.
        var source = new NodeData(".General.IfNode, LuaSTGEditorSharp");
        var then = new NodeData(".General.IfThen, LuaSTGEditorSharp");
        var otherwise = new NodeData(".General.IfElse, LuaSTGEditorSharp");
        var code = new NodeData(".General.Code, LuaSTGEditorSharp");
        source.Add(then);
        source.Add(otherwise);
        source.Add(code);
        var logger = new RecordingLogger();
        var context = new SharpNodeFormattingContext(source, "CMEX18.lstges", logger);
        context.RegisterSourceLine(source, 466);
        context.RegisterSourceLine(then, 467);
        context.RegisterSourceLine(otherwise, 469);
        context.RegisterSourceLine(code, 470);

        new IfBranchCompatibilityNormalizationConverter().Convert(source, context);

        Assert.Equal(new[] { then, code, otherwise }, source.PhysicalChildren);
        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal("CMEX18.lstges", entry.Fields["InputFilePath"]);
        Assert.Equal(466, entry.Fields["SourceLine"]);
        Assert.Equal("[IfBranchCompatibilityNormalization] [IfThen@467, IfElse@469, Code@470] "
            + "-> [IfThen@467, Code@470, IfElse@469]", entry.Fields["Diagnostic"]);
    }

    private sealed class RecordingLogger : ILogger
    {
        public List<(LogLevel Level, Dictionary<string, object?> Fields)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var fields = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(state);
            Entries.Add((logLevel, fields.ToDictionary(pair => pair.Key, pair => pair.Value)));
        }
    }
}
