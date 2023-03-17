using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public class CodeGenerationTask : BuildingTaskBase
    {
        public IInputSource Source { get; set; }
        public string? TargetVariable { get; set; }

        public CodeGenerationTask(IInputSource source, string? targetVariable = null)
        {
            Source = source;
            TargetVariable = targetVariable;
        }

        public override void Execute(BuildingContext context)
        {
            string[] path = Source.GetSource(context);
            List<string>? targets = null;
            if (TargetVariable != null)
            {
                targets = new();
            }
            foreach (string pathItem in path)
            {
                string testSrc;
                using (FileStream fs = new(pathItem, FileMode.Open, FileAccess.Read))
                {
                    using StreamReader sr = new(fs);
                    testSrc = sr.ReadToEnd();
                }
                NodeData? root = JsonConvert.DeserializeObject<NodeData>(testSrc) ?? throw new System.Exception();

                var targetName = Path.Combine(context.TemporaryFolderPath,
                    $"{Path.GetFileNameWithoutExtension(pathItem)}.lua");
                using (FileStream fs = new(targetName, FileMode.Create, FileAccess.Write))
                {
                    using StreamWriter sw = new(fs, Encoding.UTF8);
                    foreach (CodeData codeData in CodeGeneratorServiceBase.GenerateCode(root, context.LocalSettings))
                    {
                        sw.Write(codeData.Content);
                    }
                }
#pragma warning disable CS8602
                if (TargetVariable != null)
                {
                    targets.Add(targetName);
                }
            }
            if (TargetVariable != null)
            {
                context.SetVariable(TargetVariable, targets.ToArray());
            }
#pragma warning restore CS8602
        }
    }
}
