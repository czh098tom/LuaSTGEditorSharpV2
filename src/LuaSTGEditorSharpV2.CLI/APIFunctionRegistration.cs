using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.CLI
{
    public static class APIFunctionRegistration
    {
        public static void Register()
        {
            APIFunction.Register("compile", async (param) =>
            {
                if (param.InputPath != null)
                {
                    var doc = DocumentModel.CreateFromFile(param.InputPath);
                    NodeData? root = doc.Root;

                    TextWriter writer;
                    if (param.OutputPath == null)
                    {
                        writer = Console.Out;

                        foreach (CodeData codeData in CodeGeneratorServiceBase.GenerateCode(root, new LocalServiceParam(doc)))
                        {
                            await writer.WriteAsync(codeData.Content);
                        }
                    }
                    else
                    {
                        using FileStream fs = new(param.OutputPath, FileMode.OpenOrCreate);
                        using StreamWriter sr = new(fs);
                        writer = sr;

                        foreach (CodeData codeData in CodeGeneratorServiceBase.GenerateCode(root, new LocalServiceParam(doc)))
                        {
                            await writer.WriteAsync(codeData.Content);
                        }
                    }

                    //DocumentFormatBase.Create("xml").SaveToStream(root, Console.Out);
                    //Console.WriteLine();
                }

            }, typeof(CodeGeneratorServiceBase));
            APIFunction.Register("formatxml", (param) =>
            {
                if (param.InputPath != null)
                {
                    var doc = DocumentModel.CreateFromFile(param.InputPath);
                    NodeData? root = doc.Root;

                    TextWriter writer;
                    if (param.OutputPath == null)
                    {
                        writer = Console.Out;
                        DocumentFormatBase.Create("xml").SaveToStream(root, writer);
                    }
                    else
                    {
                        using FileStream fs = new(param.OutputPath, FileMode.OpenOrCreate);
                        using StreamWriter sr = new(fs);
                        writer = sr;
                        DocumentFormatBase.Create("xml").SaveToStream(root, writer);
                    }
                }
            });
        }
    }
}
