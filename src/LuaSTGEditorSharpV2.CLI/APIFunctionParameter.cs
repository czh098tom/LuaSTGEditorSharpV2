using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.CLI
{
    public class APIFunctionParameter
    {
        public static readonly string outputKey = "o";
        public static readonly string packageKey = "pkg";
        public static readonly char levelIndicator = '-';

        public static APIFunctionParameter ParseFromCommandLineArgs(string[] args)
        {
            HashSet<string> builtinKeys = new() { outputKey, packageKey };
            Dictionary<string, int> builtInkeyPositions = new();
            Dictionary<string, int> settingsKeyPositions = new();
            Dictionary<string, int> ends = new();
            Stack<int> idxLevel = new();
            idxLevel.Push(-1);
            int[] levels = new int[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                levels[i] = GetKeyLevel(args[i]);
                if (levels[i] == 1)
                {
                    if (builtinKeys.Contains(args[i].TrimStart(levelIndicator)))
                    {
                        builtInkeyPositions.Add(args[i].TrimStart(levelIndicator), i);
                    }
                    else
                    {
                        settingsKeyPositions.Add(args[i].TrimStart(levelIndicator), i);
                    }
                }
                if (levels[i] != 0)
                {
                    while (idxLevel.Count > levels[i])
                    {
                        var o = idxLevel.Pop();
                        if (o > 0) ends.Add(args[o].TrimStart(levelIndicator), i);
                    }
                    idxLevel.Push(i);
                }
            }
            while (idxLevel.Count > 0)
            {
                var o = idxLevel.Pop();
                if (o > 0) ends.Add(args[o].TrimStart(levelIndicator), args.Length);
            }
            string? ipath = args.GetOrDefault(1);
            string? opath = null;
            if (builtInkeyPositions.TryGetValue(outputKey, out int idx))
            {
                opath = args.GetOrDefault(idx + 1);
            }
            if (ipath != null) ipath = Path.GetFullPath(ipath);
            if (opath != null) opath = Path.GetFullPath(opath);
            string[]? pkg = null;
            if (builtInkeyPositions.TryGetValue(packageKey, out int idxpkg))
            {
                pkg = args[(idxpkg + 1)..ends[packageKey]];
            }

            // parse settingsEx
            Dictionary<string, JObject> settingsEx = new();
            foreach (var kvp in settingsKeyPositions)
            {
                JObject obj = new();
                Stack<JObject> objStack = new();

                bool isObj = true;
                JArray array = new();
                idxLevel.Push(kvp.Value);
                objStack.Push(obj);
                JToken item;
                for (int i = kvp.Value + 1; i < ends[kvp.Key]; i++)
                {
                    if (levels[i] > 0)
                    {
                        item = array;
                        if (array.Count == 1)
                        {
                            item = array[0];
                        }
                        else if (array.Count == 0)
                        {
                            item = "True";
                        }
                        if (levels[i] <= idxLevel.Count)
                        {
                            do
                            {
                                if (!isObj)
                                {
                                    objStack.Peek().Add(args[idxLevel.Peek()].TrimStart(levelIndicator), item);
                                }
                                else
                                {
                                    var finishedObj = objStack.Pop();
                                    var idxKey = idxLevel.Pop();
                                    objStack.Peek().Add(args[idxKey].TrimStart(levelIndicator), finishedObj);
                                }
                                isObj = true;
                            }
                            while (idxLevel.Count >= levels[i]);
                            idxLevel.Push(i);
                            array = new JArray();
                        }
                        if (levels[i] > idxLevel.Count)
                        {
                            isObj = true;
                            objStack.Push(new JObject());
                            idxLevel.Push(i);
                            array = new JArray();
                        }
                    }
                    else
                    {
                        isObj = false;
                        objStack.Pop();
                        array.Add(args[i]);
                    }
                }
                item = array;
                if (array.Count == 1)
                {
                    item = array[0];
                }
                else if (array.Count == 0)
                {
                    item = "True";
                }
                do
                {
                    if (!isObj)
                    {
                        objStack.Peek().Add(args[idxLevel.Peek()].TrimStart(levelIndicator), item);
                        idxLevel.Pop();
                    }
                    else
                    {
                        var finishedObj = objStack.Pop();
                        var idxKey = idxLevel.Pop();
                        objStack.Peek().Add(args[idxKey].TrimStart(levelIndicator), finishedObj);
                    }
                    isObj = true;
                }
                while (idxLevel.Count > 1);

                settingsEx.Add(kvp.Key, obj);
            }
            return new APIFunctionParameter(pkg, ipath, opath, settingsEx);
        }

        private static int GetKeyLevel(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != '-') return i;
            }
            return s.Length;
        }

        public IReadOnlyList<string>? Packages { get; private set; }
        public string? InputPath { get; private set; }
        public string? OutputPath { get; private set; }
        public IReadOnlyDictionary<string, JObject>? ServiceSettings { get; private set; }

        public APIFunctionParameter(IReadOnlyList<string>? packages, string? inputPath, string? outputPath
            , IReadOnlyDictionary<string, JObject>? serviceSettings)
        {
            Packages = packages;
            InputPath = inputPath;
            OutputPath = outputPath;
            ServiceSettings = serviceSettings;
        }

        public void UsePackages()
        {
            if (Packages == null) return;
            foreach (var p in Packages)
            {
                ServiceManager.LoadPackage(p);
            }
        }

        public void ReassignSettings()
        {
            if (ServiceSettings == null) return;
            foreach (var kvp in ServiceSettings)
            {
                ServiceManager.ReplaceSettingsForServiceShortNameIfValid(kvp.Key, kvp.Value);
            }
        }
    }
}
