using System.Globalization;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using Newtonsoft.Json.Linq;

namespace LuaSTGEditorSharpV2.CLI
{
    internal class Program
    {
        static Program()
        {
            APIFunction.Register("compile", (param) =>
            {
                if (param.InputPath != null)
                {
                    var doc = DocumentModel.CreateFromFile(param.InputPath);
                    NodeData? root = doc.Root;

                    //DocumentFormatBase.Create("xml").SaveToStream(root, Console.Out);
                    //Console.WriteLine();

                    foreach (CodeData codeData in CodeGeneratorServiceBase.GenerateCode(root, new LocalSettings(doc)))
                    {
                        Console.Write(codeData.Content);
                    }
                }

            }, typeof(CodeGeneratorServiceBase));
        }

        static void Main(string[] args)
        {
            string testPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\test");
            try
            {
                APIFunction.FindAndExecute("compile", new APIFunctionParameter()
                {
                    InputPath = Path.Combine(testPath, "test.lstgxml"),
                    Packages = new string[] { "Core", "LegacyNode" },
                    ServiceSettings = new Dictionary<string, JObject>()
                    {
                        { 
                            "cgen", new JObject()
                            {
                                { "indention_string", "????" }
                            } 
                        }
                    },
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}