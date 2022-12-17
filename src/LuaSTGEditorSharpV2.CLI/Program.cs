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
            Console.WriteLine(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            string testPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\test");
            try
            {
                PackageManager.UseService(typeof(CodeGeneratorServiceBase));
                PackageManager.LoadPackage(Path.Combine(testPath, "package"));

                string testSrc;
                using (FileStream fs = new(Path.Combine(testPath, "test.lstg"), FileMode.Open, FileAccess.Read))
                {
                    using StreamReader sr = new(fs);
                    testSrc = sr.ReadToEnd();
                }
                NodeData? root = JsonConvert.DeserializeObject<NodeData>(testSrc) ?? throw new Exception();

                var ctx = CodeGeneratorServiceBase.GetContextOfNode(root, new LocalSettings());
                var service = CodeGeneratorServiceBase.GetServiceOfNode(root);

                foreach (CodeData codeData in service.GenerateCodeWithContext(root, ctx))
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