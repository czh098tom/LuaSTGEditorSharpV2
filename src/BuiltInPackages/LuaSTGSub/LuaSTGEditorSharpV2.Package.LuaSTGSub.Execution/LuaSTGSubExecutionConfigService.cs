using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.Execution;

namespace LuaSTGEditorSharpV2.Package.LuaSTGSub.Execution
{
    public class LuaSTGSubExecutionConfigService(IServiceProvider serviceProvider, 
        LocalizationService localizationService) : ExecutionConfigServiceBase(serviceProvider)
    {
        [JsonProperty] public NodePropertyCapture? NameCapture { get; private set; }
        [JsonProperty] public NodePropertyCapture? ModNameCapture { get; private set; }
        [JsonProperty] public NodePropertyCapture? IsWindowedCapture { get; private set; }
        [JsonProperty] public NodePropertyCapture? ResolutionXCapture { get; private set; }
        [JsonProperty] public NodePropertyCapture? ResolutionYCapture { get; private set; }
        [JsonProperty] public NodePropertyCapture? IsCheatCapture { get; private set; }

        public override ExecutionConfig? GetExecutionConfig(NodeData nodeData, ExecutionConfigContext context)
        {
            NodePropertyAccessToken token = new(ServiceProvider, nodeData, context);
            var name = NameCapture?.Capture(token) ?? string.Empty;
            return new ExecutionConfig(name, (prog, c) => ExecutionTaskFactory(nodeData, context, prog, c));
        }

        private async Task ExecutionTaskFactory(NodeData nodeData, ExecutionConfigContext context, IProgress<string> progressReporter, CancellationToken cancellationToken)
        {
            var redirectStandardOutput = false;
            var luaSTGPath = context.Settings.TargetExecutablePath;
            if (string.IsNullOrEmpty(luaSTGPath)) throw new ExecutionException();
            var baseDirectory = Path.GetDirectoryName(luaSTGPath);
            if (string.IsNullOrEmpty(baseDirectory)) throw new ExecutionException();
            var logFileName = "engine.log";

            NodePropertyAccessToken token = new(ServiceProvider, nodeData, context);
            var modName = ModNameCapture?.Capture(token) ?? string.Empty;
            var isWindowed = IsWindowedCapture?.Capture(token)?.ToLower() ?? "true";
            var resolutionX = ResolutionXCapture?.Capture(token) ?? "800";
            var resolutionY = ResolutionYCapture?.Capture(token) ?? "600";
            var isCheat = IsCheatCapture?.Capture(token)?.ToLower() ?? "false";

            var parameter = "\""
                + "start_game=true is_debug=true setting.nosplash=true setting.windowed="
                + isWindowed + " setting.resx=" + resolutionX
                + " setting.resy=" + resolutionY + " cheat=" + isCheat
                + " setting.mod=\'" + modName + "\'\"";

            var lstgInstance = new Process
            {
                StartInfo = new ProcessStartInfo(luaSTGPath, parameter)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = baseDirectory,
                    RedirectStandardError = false,
                    RedirectStandardOutput = redirectStandardOutput,
                    StandardErrorEncoding = null,
                    StandardOutputEncoding = redirectStandardOutput ? Encoding.UTF8 : null,
                },
                EnableRaisingEvents = true
            };

            if (redirectStandardOutput)
            {
                lstgInstance.OutputDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                    {
                        progressReporter.Report(e.Data);
                    }
                };
                lstgInstance.Exited += (s, e) =>
                {
                    progressReporter.Report("\n");
                    progressReporter.Report(string.Format(
                        localizationService.GetString("log_execution_exited", 
                            typeof(LuaSTGSubExecutionConfigService).Assembly), lstgInstance.ExitCode));
                };
            }
            else
            {
                lstgInstance.Exited += (s, e) =>
                {
                    StringBuilder sb = new();
                    using (var fs = new FileStream(Path.GetFullPath(
                        Path.Combine(baseDirectory, logFileName)), FileMode.Open))
                    {
                        using var sr = new StreamReader(fs);
                        int i = 0;
                        while (!sr.EndOfStream && i < 8192)
                        {
                            sb.Append(sr.ReadLine());
                            sb.Append('\n');
                            i++;
                        }
                        progressReporter.Report(sb.ToString());
                    }
                    sb.Append('\n');
                    sb.Append(string.Format(
                        localizationService.GetString("log_execution_exited",
                            typeof(LuaSTGSubExecutionConfigService).Assembly), lstgInstance.ExitCode));
                    sb.Append('\n');
                    progressReporter.Report(sb.ToString());
                };
            }

            lstgInstance.Start();

            progressReporter.Report(localizationService.GetString("log_execution_running", typeof(LuaSTGSubExecutionConfigService).Assembly));
            progressReporter.Report("\n\n");

            if (redirectStandardOutput)
            {
                lstgInstance.BeginOutputReadLine();
            }

            while (!lstgInstance.HasExited)
            {
                await Task.Delay(100, cancellationToken);
            }
        }
    }
}
