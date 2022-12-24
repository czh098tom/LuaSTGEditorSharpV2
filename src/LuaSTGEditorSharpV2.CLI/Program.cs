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

                string testSrc;
                using (FileStream fs = new(Path.Combine(testPath, "test.lstg"), FileMode.Open, FileAccess.Read))
                {
                    using StreamReader sr = new(fs);
                    testSrc = sr.ReadToEnd();
                }
                NodeData? root = JsonConvert.DeserializeObject<NodeData>(testSrc) ?? throw new Exception();

                foreach (CodeData codeData in CodeGeneratorServiceBase.GenerateCode(root, new LocalSettings()))
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