using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.CLI
{
    public class APIFunctionParameterResolver
    {
        public static readonly string packageKey = "pkg";
        public static readonly string outputKey = "o";
        public static readonly string taskKey = "t";
        public static readonly char levelIndicator = '-';

        private readonly HashSet<string> builtinKeys = [packageKey, outputKey, taskKey];
        private Dictionary<string, int> builtInkeyPositions = [];
        private Dictionary<string, int> settingsKeyPositions = [];
        private Dictionary<string, int> ends = [];
        private Stack<int> idxLevel = new();

        private int[] levels = [];

        private Dictionary<string, JObject> settingsEx = [];

        private Dictionary<JToken, int> jvalue2Idx = [];
        private Dictionary<JObject, int> jobj2Idx = [];
        private Stack<JObject> objs = new();
        private Stack<int> idxs = new();
        private Stack<string> keys = new();

        public APIFunctionParameter Resolve(string[] args)
        {
            builtInkeyPositions = [];
            settingsKeyPositions = [];
            ends = [];
            idxLevel = new();
            levels = new int[args.Length];

            idxLevel.Push(-1);

            var builtIn = ResolveBuiltIn(args);
            Dictionary<string, JObject> settingsEx = ResolveSettingsEX(args);
            return new APIFunctionParameter(builtIn, settingsEx);
        }

        private BuiltInParams ResolveBuiltIn(string[] args)
        {
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
            if (builtInkeyPositions.TryGetValue(outputKey, out int idxOutput))
            {
                opath = args.GetOrDefault(idxOutput + 1);
            }
            if (ipath != null) ipath = Path.GetFullPath(ipath);
            if (opath != null) opath = Path.GetFullPath(opath);
            string[]? pkg = null;
            if (builtInkeyPositions.TryGetValue(packageKey, out int idxpkg))
            {
                pkg = args[(idxpkg + 1)..ends[packageKey]];
            }
            string? task = null;
            if (builtInkeyPositions.TryGetValue(taskKey, out int idxTask))
            {
                task = args.GetOrDefault(idxTask + 1);
            }
            return new BuiltInParams(pkg, ipath, opath, task);
        }

        private Dictionary<string, JObject> ResolveSettingsEX(string[] args)
        {
            settingsEx = new();
            foreach (var kvp in settingsKeyPositions)
            {
                jvalue2Idx = new(ReferenceEqualityComparer.Instance);
                jobj2Idx = [];
                objs = new();
                idxs = new();
                keys = new();
                objs.Push([]);
                idxs.Push(kvp.Value);
                keys.Push(kvp.Key);
                for (int i = kvp.Value + 1; i < ends[kvp.Key]; i++)
                {
                    if (levels[i] > 0 && levels[i] <= levels[i - 1])
                    {
                        bool once = false;
                        while (levels[i] <= levels[idxs.Peek()] || (levels[idxs.Peek()] == 0 && !once))
                        {
                            ParseJArrayOrJValue();
                            once = true;
                        }
                    }
                    JObject maybeObj = new();
                    string key = args[i].TrimStart('-');
                    jobj2Idx.Add(maybeObj, i);
                    objs.Peek().Add(key, maybeObj);
                    objs.Push(maybeObj);
                    keys.Push(key);
                    idxs.Push(i);
                }
                while (idxs.Count > 1)
                {
                    ParseJArrayOrJValue();
                }
                settingsEx.Add(kvp.Key, objs.Peek());
            }

            return settingsEx;
        }

        private void ParseJArrayOrJValue()
        {
            var lastobj = objs.Pop();
            var lastidx = idxs.Pop();
            var lastkey = keys.Pop();
            if (lastobj.HasValues)
            {
                bool allLiteral = true;
                foreach (var kvpobj in lastobj)
                {
                    // TODO: on detecting mixed structure (by index == 0), throw an exception
                    if (kvpobj.Value is not JValue b || b.Type != JTokenType.Boolean)
                    {
                        allLiteral = false;
                        break;
                    }
                }
                if (allLiteral)
                {
                    if (lastobj.Count == 1)
                    {
                        // single value
                        foreach (var kvpobj in lastobj)
                        {
                            objs.Peek().Remove(lastkey);
                            JValue jValue = new(kvpobj.Key);
                            objs.Peek().Add(lastkey, jValue);
                            break;
                        }
                    }
                    else
                    {
                        // jarray
                        JArray arr = new();
                        objs.Peek().Remove(lastkey);
                        foreach (var kvpobj in lastobj)
                        {
                            JValue jValue = new(kvpobj.Key);
                            arr.Add(jValue);
                        }
                        objs.Peek().Add(lastkey, arr);
                    }
                }
            }
            else
            {
                // key only
                objs.Peek().Remove(lastkey);
                JValue jValue = new(true);
                jvalue2Idx.Add(jValue, jobj2Idx[lastobj]);
                objs.Peek().Add(lastkey, jValue);
            }
        }

        private static int GetKeyLevel(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != '-') return i;
            }
            return s.Length;
        }
    }
}
