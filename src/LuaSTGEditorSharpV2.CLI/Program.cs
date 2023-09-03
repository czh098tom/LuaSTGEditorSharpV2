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
        static void Main(string[] args)
        {
            APIFunctionRegistration.Register();
            try
            {
                var param = APIFunctionParameter.ParseFromCommandLineArgs(args);
                APIFunction.FindAndExecute(args[0], param);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}