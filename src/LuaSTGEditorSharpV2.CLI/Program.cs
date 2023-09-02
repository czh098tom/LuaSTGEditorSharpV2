using System.Globalization;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.CodeGenerator;

namespace LuaSTGEditorSharpV2.CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string testPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\test");
            try
            {
                PackageManager.UseService(typeof(CodeGeneratorServiceBase));
                var resc = PackageManager.LoadPackage("Core");
                var resln = PackageManager.LoadPackage("LegacyNode");

                var doc = DocumentModel.CreateFromFile(Path.Combine(testPath, "test.lstgxml"));
                NodeData? root = doc.Root;

                DocumentFormatBase.Create("xml").SaveToStream(root, Console.Out);
                Console.WriteLine();

                foreach (CodeData codeData in CodeGeneratorServiceBase.GenerateCode(root, new LocalSettings(doc)))
                {
                    Console.Write(codeData.Content);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}